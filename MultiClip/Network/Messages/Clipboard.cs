using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MultiClip.Clipboard;
using MultiClip.Models;
using MultiClip.Utilities;

namespace MultiClip.Network.Messages
{
    public class ClipboardStateInfo
    {
        public Guid Id { get; set; }
        public int Size { get; set; }
        public AbstractDataFormat Format { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class ClipboardInfoRequest : Request<ClipboardInfoRequest, ClipboardInfoResponse>
    {
        private const int Descriptor = 782511482;

        public List<Guid> KnownStateGuids { get; }

        public ClipboardInfoRequest(List<Guid> knownStates)
        {
            KnownStateGuids = knownStates;
        }

        public override IResponse GetResponse(IPAddress remoteIP)
        {
            return ClipboardInfoResponse.GetResponse(KnownStateGuids);
        }
    }

    public class ClipboardInfoResponse : Response<ClipboardInfoResponse>
    {
        public List<ClipboardStateInfo> States { get; set; }

        public static ClipboardInfoResponse GetResponse(List<Guid> knownStateGuids)
        {
            return new ClipboardInfoResponse
            {
                States = (knownStateGuids != null
                    ? AppState.Current.LocalStates.Where(x => !knownStateGuids.Contains(x.Id))
                    : AppState.Current.LocalStates)
                    .Select(x =>
                    {
                        ClipboardItem item = ClipboardParser.GetPreferredItem(x.Items, serializable: true);
                        if (item != null)
                        {
                            return new ClipboardStateInfo
                            {
                                Id = x.Id,
                                Size = item.Size,
                                Format = ClipboardParser.GetAbstractFormat(item.Format),
                                DateTime = x.DateTime,
                            };
                        }
                        else
                        {
                            return null;
                        }
                    })
                    .Where(x => x != null)
                    .ToList(),
            };
        }
    }

    public class ClipboardStateRequest : Request<ClipboardStateRequest, ClipboardStateResponse>
    {
        private const int Descriptor = 13352312;

        public Guid StateGuid { get; set; }

        public ClipboardStateRequest(Guid stateGuid)
        {
            StateGuid = stateGuid;
        }

        public override IResponse GetResponse(IPAddress remoteIP)
        {
            return ClipboardStateResponse.GetResponse(StateGuid);
        }
    }

    public class ClipboardStateResponse : Response<ClipboardStateResponse>
    {
        public Guid StateGuid { get; set; }
        public byte[] Buffer { get; set; }
        public DataFormat Format { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsError { get; set; }

        public static ClipboardStateResponse GetResponse(Guid stateId)
        {
            ClipboardState state = AppState.Current.LocalStates.FirstOrDefault(x => x.Id == stateId);
            ClipboardItem item = state?.Items.FirstOrDefault(
                x => ClipboardParser.CanSerialize(ClipboardParser.GetAbstractFormat(x.Format)));

            if (state != null && item != null)
            {
                return new ClipboardStateResponse
                {
                    StateGuid = stateId,
                    Buffer = item.GetDataBuffer(),
                    Format = item.Format,
                    DateTime = state.DateTime,
                    IsError = false,
                };
            }
            else
            {
                return new ClipboardStateResponse
                {
                    StateGuid = stateId,
                    Buffer = null,
                    Format = (DataFormat)0,
                    DateTime = state.DateTime,
                    IsError = true,
                };
            }
        }
    }

    public class ClipboardNotifyChangedRequest : Request<ClipboardNotifyChangedRequest, ClipboardNotifyChangedResponse>
    {
        private const int Descriptor = 1654312;

        public Guid StateGuid { get; set; }

        public ClipboardNotifyChangedRequest(Guid stateId)
        {
            StateGuid = stateId;
        }

        public override IResponse GetResponse(IPAddress remoteIP)
        {
            return ClipboardNotifyChangedResponse.GetResponse(remoteIP, StateGuid);
        }
    }

    public class ClipboardNotifyChangedResponse : Response<ClipboardNotifyChangedResponse>
    {
        public static ClipboardNotifyChangedResponse GetResponse(IPAddress remoteIP, Guid stateId)
        {
            Task.Run(async () =>
            {
                var endPoint = new IPEndPoint(remoteIP, NetConfig.Port);
                var host = AppState.Current.RemoteClipboardStates.FirstOrDefault(x => x.Identity.EndPoint.Address.Equals(endPoint.Address));
                if (host != null)
                {
                    var res = await host.Identity.SendAsync(new ClipboardStateRequest(stateId));
                    if (res.IsError)
                    {
                        Logger.Default.LogWarn(LogEvents.NetErr, res);
                    }
                    else
                    {
                        host.States.Add(new ClipboardState(res.StateGuid, res.DateTime, new ClipboardItem[] 
                        {
                            ClipboardItem.FromBuffer(res.Format, res.Buffer, cloneBuffer: false)
                        }));
                    }
                }
            });
            return new ClipboardNotifyChangedResponse();
        }
    }

}
