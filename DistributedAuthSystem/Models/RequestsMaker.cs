﻿using DistributedAuthSystem.Constants;
using DistributedAuthSystem.Logger;
using DistributedAuthSystem.Requests;
using DistributedAuthSystem.Responses;
using DistributedAuthSystem.Services;
using DistributedAuthSystem.States;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace DistributedAuthSystem.Models
{
    public class RequestsMaker
    {
        #region fields

        private readonly IClientsRepository _clientsRepository;

        private readonly INeighboursRepository _neighboursRepository;

        private readonly ISynchronizationsRepository _synchronizationsRepository;

        private readonly IServerInfoRepository _serverInfoRepository;

        private readonly JavaScriptSerializer _serializer;

        private const int _asyncReqtimeout = 10000;

        private const int _checkPassTimeout = 10000;

        private const string _thinEndpoint = "private/synchro/thin";

        private const string _fatEndpoint = "private/synchro/fat";

        private const string _checkPassEndpoint = "public/clients/{0}/checkpass";

        #endregion

        #region methods

        public RequestsMaker(IClientsRepository clientsRepository, INeighboursRepository neighboursRepository,
            ISynchronizationsRepository synchronizationsRepository, IServerInfoRepository serverInfoRepository)
        {
            _clientsRepository = clientsRepository;
            _neighboursRepository = neighboursRepository;
            _synchronizationsRepository = synchronizationsRepository;
            _serverInfoRepository = serverInfoRepository;
            _serializer = new JavaScriptSerializer();
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                HttpWebRequest request = state as HttpWebRequest;
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        private void FatCallback(IAsyncResult ar)
        {
            RequestState requestState = (RequestState)ar.AsyncState;
            WebRequest request = requestState.Request;
            WebResponse response = request.EndGetResponse(ar);

            FatSynchronizationRes fatResponse;

            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
                fatResponse = _serializer.Deserialize<FatSynchronizationRes>(responseString);
            }

            if (fatResponse.Type != FatSynchroResult.CONFLICT)
            {
                _synchronizationsRepository.UpdateTime(fatResponse.SenderId, fatResponse.SynchroTimestamp);
                _synchronizationsRepository.UpdateSynchroTimes(fatResponse.SynchroTimes, fatResponse.SenderId,
                    fatResponse.SynchroTimestamp);
            }

            if (fatResponse.Type == FatSynchroResult.CONFLICT ||
                fatResponse.Type == FatSynchroResult.U2OLD)
            {
                if (fatResponse.RequestTimestamp > _serverInfoRepository.GetLastFatSynchro())
                {
                    var serverState = fatResponse.Type == FatSynchroResult.CONFLICT
                        ? ServerState.IS_IN_CONFLICT : ServerState.IS_2OLD;

                    _serverInfoRepository.SetServerState(serverState);
                }
            }
        }

        private void GetFatRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            StreamData streamData = (StreamData)asynchronousResult.AsyncState;

            var request = streamData.Request;
            var data = streamData.Data;

            Stream postStream;

            try
            {
                postStream = request.EndGetRequestStream(asynchronousResult);
            }
            catch (Exception)
            {
                return;
            }

            using (var streamWriter = new StreamWriter(postStream))
            {
                streamWriter.Write(data);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var requestState = new RequestState
            {
                Request = request
            };

            IAsyncResult result = request.BeginGetResponse(new AsyncCallback(FatCallback), requestState);
        }

        public void SendFatRequest(string serverId)
        {
            long requestTimestamp = OperationsLog.GenerateTimestamp();

            Dictionary<string, long> synchroTimesCopy;
            _synchronizationsRepository.GetSynchroTimesCopy(out synchroTimesCopy);

            var fatRequest = new FatSynchronizationReq
            {
                SenderId = _serverInfoRepository.GetServerId(),
                History = _clientsRepository.GetHistorySince(synchroTimesCopy[serverId]),
                SynchroTimes = synchroTimesCopy,
                RequestTimestamp = requestTimestamp
            };

            var data = _serializer.Serialize(fatRequest);
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            var url = _neighboursRepository.GetSingleNeighbour(serverId).Url;
            WebRequest wreq = WebRequest.Create("http://" + url + "/" + _fatEndpoint);
            wreq.Method = "POST";
            wreq.ContentType = "application/json";
            wreq.ContentLength = bytes.Length;
            wreq.Timeout = _asyncReqtimeout;

            var streamData = new StreamData
            {
                Request = wreq,
                Data = data
            };

            wreq.BeginGetRequestStream(new AsyncCallback(GetFatRequestStreamCallback), streamData);
        }

        public void SendFatRequestsToAll()
        {
            var neighbours = _neighboursRepository.GetAllNeighbours();
            foreach (var neigh in neighbours)
            {
                SendFatRequest(neigh.Id);
            }
        }

        private void ThinCallback(IAsyncResult ar)
        {
            RequestState requestState = (RequestState)ar.AsyncState;
            WebRequest request = requestState.Request;
            WebResponse response = request.EndGetResponse(ar);

            ThinSynchronizationRes thinResponse;

            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                String responseString = reader.ReadToEnd();
                thinResponse = _serializer.Deserialize<ThinSynchronizationRes>(responseString);
            }

            if (thinResponse.Type == ThinSynchroResult.ALREADY_SYNC)
            {
                _synchronizationsRepository.UpdateTime(thinResponse.SenderId, thinResponse.SynchroTimestamp);
                _synchronizationsRepository.UpdateSynchroTimes(thinResponse.SynchroTimes, thinResponse.SenderId,
                    thinResponse.SynchroTimestamp);
                return;
            }

            SendFatRequest(thinResponse.SenderId);
        }

        private void GetThinRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            StreamData streamData = (StreamData)asynchronousResult.AsyncState;

            var request = streamData.Request;
            var data = streamData.Data;

            Stream postStream;

            try
            {
                postStream = request.EndGetRequestStream(asynchronousResult);
            }
            catch (Exception)
            {
                return;
            }

            using (var streamWriter = new StreamWriter(postStream))
            {
                streamWriter.Write(data);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var requestState = new RequestState
            {
                Request = request
            };

            IAsyncResult result = request.BeginGetResponse(new AsyncCallback(ThinCallback), requestState);
        }

        public void SendThinRequestsToAll()
        {
            var lastOperationTsmp = _clientsRepository.GetLastOperationTsmp();
            var neighbours = _neighboursRepository.GetAllNeighbours();

            Dictionary<string, long> synchroTimesCopy;
            _synchronizationsRepository.GetSynchroTimesCopy(out synchroTimesCopy);

            foreach (var neigh in neighbours)
            {
                if (synchroTimesCopy[neigh.Id] < lastOperationTsmp)
                {
                    long requestTimestamp = OperationsLog.GenerateTimestamp();

                    var thinRequest = new ThinSynchronizationReq
                    {
                        SenderId = _serverInfoRepository.GetServerId(),
                        LastHash = _clientsRepository.GetLastHash(),
                        SynchroTimestamp = _clientsRepository.GetLastOperationTsmp(),
                        SynchroTimes = synchroTimesCopy,
                        RequestTimestamp = requestTimestamp
                    };

                    var data = _serializer.Serialize(thinRequest);
                    byte[] bytes = Encoding.UTF8.GetBytes(data);

                    WebRequest wreq = WebRequest.Create("http://" + neigh.Url + "/" + _thinEndpoint);
                    wreq.Method = "POST";
                    wreq.ContentType = "application/json";
                    wreq.ContentLength = bytes.Length;
                    wreq.Timeout = _asyncReqtimeout;

                    var streamData = new StreamData
                    {
                        Request = wreq,
                        Data = data
                    };

                    wreq.BeginGetRequestStream(new AsyncCallback(GetThinRequestStreamCallback), streamData);
                }
            }
        }

        public bool CheckCurrentPassword(string id, string pin, string oneTimePassword)
        {
            var neighbours = _neighboursRepository.GetAllNeighbours();
            foreach (var neigh in neighbours)
            {
                if (!neigh.IsSpecial)
                {
                    continue;
                }

                var authPasswordRequest = new AuthPasswordReq
                {
                    Pin = pin,
                    OneTimePassword = oneTimePassword
                };

                var data = _serializer.Serialize(authPasswordRequest);
                byte[] bytes = Encoding.UTF8.GetBytes(data);

                WebRequest wreq = WebRequest.Create("http://" + neigh.Url + "/" +
                    String.Format(_checkPassEndpoint, id.ToString()));
                wreq.Method = "POST";
                wreq.ContentType = "application/json";
                wreq.ContentLength = bytes.Length;
                wreq.Timeout = _checkPassTimeout;

                using (var streamWriter = new StreamWriter(wreq.GetRequestStream()))
                {
                    streamWriter.Write(data);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                try
                {
                    WebResponse response = wreq.GetResponse();
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        var resp = (HttpWebResponse)e.Response;
                        if (resp.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return false;
                        }
                    }

                    throw e;
                }
            }

            return true;
        }

        #endregion
    }
}