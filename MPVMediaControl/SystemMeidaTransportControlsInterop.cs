// From https://github.com/AdamBraden/WindowsInteropWrappers/blob/master/CoreWindowInterop/SystemMediaTransportControlsInterop.cs
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media;

namespace UWPInterop
{
    //MIDL_INTERFACE("ddb0472d-c911-4a1f-86d9-dc3d71a95f5a")
    //ISystemMediaTransportControlsInterop : public IInspectable
    //{
    //public:
    //    virtual HRESULT STDMETHODCALLTYPE GetForWindow(
    //        /* [in] */ __RPC__in HWND appWindow,
    //        /* [in] */ __RPC__in REFIID riid,
    //        /* [iid_is][retval][out] */ __RPC__deref_out_opt void** mediaTransportControl) = 0;

    //};
    [System.Runtime.InteropServices.Guid("ddb0472d-c911-4a1f-86d9-dc3d71a95f5a")]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIInspectable)]
    interface ISystemMediaTransportControlsInterop
    {
        SystemMediaTransportControls GetForWindow(IntPtr appWindow, [System.Runtime.InteropServices.In] ref Guid riid);
    }

    //Helper to initialize SystemMediaTransportControls
    public static class SystemMediaTransportControlsInterop
    {
        public static SystemMediaTransportControls GetForWindow(IntPtr hWnd)
        {
            ISystemMediaTransportControlsInterop systemMediaTransportControlsInterop = (ISystemMediaTransportControlsInterop)WindowsRuntimeMarshal.GetActivationFactory(typeof(SystemMediaTransportControls));
            Guid guid = new Guid("99FA3FF4-1742-42A6-902E-087D41F965EC");

            return systemMediaTransportControlsInterop.GetForWindow(hWnd, ref guid);
        }
    }
}