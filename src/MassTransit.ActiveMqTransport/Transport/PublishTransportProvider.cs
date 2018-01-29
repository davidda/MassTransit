﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.ActiveMqTransport.Transport
{
    using System;
    using System.Threading.Tasks;
    using Apache.NMS;
    using GreenPipes.Agents;
    using Pipeline;
    using Topology;
    using Transports;


    public class PublishTransportProvider :
        IPublishTransportProvider
    {
        readonly ActiveMqHost _host;
        readonly IActiveMqPublishTopology _publishTopology;

        public PublishTransportProvider(ActiveMqHost host, IActiveMqPublishTopology publishTopology)
        {
            _publishTopology = publishTopology;
            _host = host;
        }

        public Task<ISendTransport> GetPublishTransport<T>(Uri publishAddress)
            where T : class
        {
            IActiveMqMessagePublishTopology<T> publishTopology = _publishTopology.GetMessageTopology<T>();

            var sendSettings = publishTopology.GetSendSettings();

            var brokerTopology = publishTopology.GetBrokerTopology(PublishBrokerTopologyOptions.MaintainHierarchy);

            IAgent<SessionContext> modelSource = GetModelSource();

            var sendTransport = new ActiveMqSendTransport(modelSource, new ConfigureTopologyFilter<SendSettings>(sendSettings, brokerTopology), sendSettings.EntityName,
                DestinationType.Topic);

            _host.Add(sendTransport);

            return Task.FromResult<ISendTransport>(sendTransport);
        }

        protected virtual IAgent<SessionContext> GetModelSource()
        {
            return new ActiveMqSessionCache(_host, _host.ConnectionCache);
        }
    }
}