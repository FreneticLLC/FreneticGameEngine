//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using FGECore.CoreSystems;
using FGEGraphics.GraphicsHelpers;

namespace FGEGraphics.AudioSystem.AudioInternals;

/// <summary>Helper for audio playback using WASAPI (Windows Audio Session API). As the name implies, this is Windows-only, however it is a very direct proper native lib, just one thin OS level above the hardware driver.</summary>
public partial class WasApiAudioProvider : GenericAudioBacker
{
    /// <summary>Extremely Windows-only native WASAPI calls.
    /// Do not touch these unless you super duper extra especially know what you're doing.
    /// Want to remove that unused function over there? Now you have a memory access violation exception somewhere. Why? Because C++ interop says Fuck You. I'm not joking that will actually happen. So seriously do not touch.
    /// <para>
    /// Documentation within is copied from Learn.Microsoft.com, and is copyright Microsoft
    /// </para></summary>
    public static partial class Native
    {
        /// <summary>Initializes the COM library for use by the calling thread, sets the thread's concurrency model, and creates a new apartment for the thread if one is required.</summary>
        /// <param name="pvReserved">This parameter is reserved and must be NULL.</param>
        /// <param name="dwCoInit">The concurrency model and initialization options for the thread. Values for this parameter are taken from the COINIT enumeration. Any combination of values from COINIT can be used, except that the COINIT_APARTMENTTHREADED and COINIT_MULTITHREADED flags cannot both be set. The default is COINIT_MULTITHREADED.</param>
        [LibraryImport("ole32.dll")]
        public static partial int CoInitializeEx(nint pvReserved, uint dwCoInit);

        /// <summary>Creates and default-initializes a single object of the class associated with a specified CLSID.</summary>
        /// <param name="rclsid">The CLSID associated with the data and code that will be used to create the object.</param>
        /// <param name="pUnkOuter">If NULL, indicates that the object is not being created as part of an aggregate. If non-NULL, pointer to the aggregate object's IUnknown interface (the controlling IUnknown).</param>
        /// <param name="dwClsContext">Context in which the code that manages the newly created object will run. The values are taken from the enumeration CLSCTX.</param>
        /// <param name="riid">A reference to the identifier of the interface to be used to communicate with the object.</param>
        /// <param name="ppv">Address of pointer variable that receives the interface pointer requested in riid. Upon successful return, *ppv contains the requested interface pointer. Upon failure, *ppv contains NULL.</param>
        [LibraryImport("ole32.dll")]
        public static partial int CoCreateInstance(ref Guid rclsid, nint pUnkOuter, uint dwClsContext, ref Guid riid, out nint ppv);

        /// <summary>Frees a block of task memory previously allocated through a call to the CoTaskMemAlloc or CoTaskMemRealloc function.</summary>
        /// <param name="pv">A pointer to the memory block to be freed. If this parameter is NULL, the function has no effect.</param>
        [LibraryImport("ole32.dll")]
        public static partial void CoTaskMemFree(nint pv);

        /// <summary>The PropVariantClear function frees all elements that can be freed in a given PROPVARIANT structure. For complex elements with known element pointers, the underlying elements are freed prior to freeing the containing element.</summary>
        /// <param name="pvar">A pointer to an initialized PROPVARIANT structure for which any deallocatable elements are to be freed. On return, all zeroes are written to the PROPVARIANT structure.</param>
        [LibraryImport("ole32.dll")]
        public static partial int PropVariantClear(nint pvar);

        /// <summary>Enum constant: Class Context: All.</summary>
        public const uint CLSCTX_ALL = 1;

        /// <summary>Determines the concurrency model used for incoming calls to objects created by this thread. This concurrency model can be either apartment-threaded or multithreaded.</summary>
        public enum COINIT : uint
        {
            /// <summary>Enum constant: Initializes the thread for apartment-threaded object concurrency (see Remarks).</summary>
            APARTMENTTHREADED = 0x2,
            /// <summary>Enum constant: Initializes the thread for multi-threaded object concurrency (see Remarks).</summary>
            MULTITHREADED = 0x0,
            /// <summary>Enum constant: Disables DDE for OLE1 support.</summary>
            DISABLE_OLE1DDE = 0x4,
            /// <summary>Enum constant: Increase memory usage in an attempt to increase performance.</summary>
            SPEED_OVER_MEMORY = 0x8
        }

        /// <summary>The AUDCLNT_SHAREMODE enumeration defines constants that indicate whether an audio stream will run in shared mode or in exclusive mode.</summary>
        public enum AUDCLNT_SHAREMODE : int
        {
            /// <summary>The audio stream will run in shared mode. For more information, see Remarks.</summary>
            SHARED = 0,
            /// <summary>The audio stream will run in exclusive mode. For more information, see Remarks.</summary>
            EXCLUSIVE = 1
        }

        /// <summary>Specifies characteristics that a client can assign to an audio stream during the initialization of the stream.</summary>
        public enum AUDCLNT_STREAMFLAGS : uint
        {
            /// <summary>A channel matrixer and a sample rate converter are inserted as necessary to convert between the uncompressed format supplied to IAudioClient::Initialize and the audio engine mix format.</summary>
            AUTOCONVERTPCM = 0x80000000,
            /// <summary>When used with AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM, a sample rate converter with better quality than the default conversion but with a higher performance cost is used. This should be used if the audio is ultimately intended to be heard by humans as opposed to other scenarios such as pumping silence or populating a meter.</summary>
            SRC_DEFAULT_QUALITY = 0x08000000
        }

        /// <summary>Storage Access constants.</summary>
        public enum STGMConstants : int
        {
            /// <summary>Read-only access.</summary>
            READ = 0,
            /// <summary>Write-only access.</summary>
            WRITE = 1,
            /// <summary>Read+Write access.</summary>
            READWRITE = 2
        }

        /// <summary>Specifies the FMTID/PID identifier that programmatically identifies a property. Replaces SHCOLUMNID.</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PropertyKey(Guid fmtid, uint pid)
        {
            /// <summary>A unique GUID for the property.</summary>
            public Guid fmtid = fmtid;

            /// <summary>A property identifier (PID). This parameter is not used as in SHCOLUMNID. It is recommended that you set this value to PID_FIRST_USABLE. Any value greater than or equal to 2 is acceptable.</summary>
            public uint pid = pid;
        }

        /// <summary>This interface exposes methods used to enumerate and manipulate property values.</summary>
        [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore
        {
            /// <summary>This method returns a count of the number of properties that are attached to the file.</summary>
            /// <param name="cProps">A pointer to a value that indicates the property count.</param>
            public int GetCount(out uint cProps);

            /// <summary>Gets a property key from the property array of an item.</summary>
            /// <param name="iProp">The index of the property key in the array of PROPERTYKEY structures. This is a zero-based index.</param>
            /// <param name="pkey">TBD (wtf actual microsoft docs here is 'TBD'??)</param>
            public int GetAt([In] uint iProp, out PropertyKey pkey);

            /// <summary>This method retrieves the data for a specific property.</summary>
            /// <param name="key">TBD</param>
            /// <param name="pv">After the IPropertyStore::GetValue method returns successfully, this parameter points to a PROPVARIANT structure that contains data about the property.</param>
            public int GetValue([In] ref PropertyKey key, [In] nint pv);

            /// <summary>This method sets a property value or replaces or removes an existing value.</summary>
            /// <param name="key">TBD</param>
            /// <param name="pv">REFPROPVARIANT - TBD</param>
            public int SetValue([In] ref PropertyKey key, [In] nint pv);

            /// <summary>After a change has been made, this method saves the changes.</summary>
            public int Commit();
        }

        /// <summary>Contains static DevPKey values.</summary>
        public static class PKeys
        {
            // ============= Core Properties that matter =============
            /// <summary>The PKEY_Device_FriendlyName property contains the friendly name of the endpoint device (for example, "Speakers (XYZ Audio Adapter)").</summary>
            public static PropertyKey PKEY_Device_FriendlyName = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 14); // STRING
            /// <summary>The device description of the endpoint device (for example, "Speakers").</summary>
            public static PropertyKey PKEY_Device_DeviceDesc = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 2); // STRING
            /// <summary>Stores the audio endpoint device instance identifier. The value can also be aquired via IMMDevice::GetId method. For more information about this property, see Endpoint ID Strings and DEVPKEY_Device_InstanceId.</summary>
            public static PropertyKey PKEY_Device_InstanceId = new(new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57), 256); // STRING
            /// <summary>The PKEY_AudioEndpoint_FormFactor property specifies the form factor of the audio endpoint device. The form factor indicates the physical attributes of the audio endpoint device that the user manipulates.</summary>
            public static PropertyKey PKEY_AudioEndpoint_FormFactor = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 0); // UI4
            /// <summary>The PKEY_AudioEndpoint_PhysicalSpeakers property specifies the channel-configuration mask for the audio endpoint device. The mask indicates the physical configuration of a set of speakers and specifies the assignment of channels to speakers. For more information about channel-configuration masks, see KSPROPERTY_AUDIO_CHANNEL_CONFIG.</summary>
            public static PropertyKey PKEY_AudioEndpoint_PhysicalSpeakers = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 3); // UI4
            /// <summary>A GUID associated with this audio endpoint, unique across all audio endpoints. This GUID can be used as the device identifier in the DirectSound APIs.</summary>
            public static PropertyKey PKEY_AudioEndpoint_GUID = new(new Guid(0x1da5d803, 0xd492, 0x4edd, 0x8c, 0x23, 0xe0, 0xc0, 0xff, 0xee, 0x7f, 0x0e), 4); // LPWSTR (stringized GUID)
            /// <summary>The device format (can be PCM integer).</summary>
            public static PropertyKey PKEY_AudioEngine_DeviceFormat = new(new Guid(0xf19f064d, 0x82c, 0x4e27, 0xbc, 0x73, 0x68, 0x82, 0xa1, 0xbb, 0x8e, 0x4c), 0); // VT_BLOB
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            // ============= others =============
            public static PropertyKey PKEY_NAME = new(new Guid(0xb725f130, 0x47ef, 0x101a, 0xa5, 0xf1, 0x02, 0x60, 0x8c, 0x9e, 0xeb, 0xac), 10); // STRING
            public static PropertyKey PKEY_Device_HardwareIds = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 3); // STRING_LIST
            public static PropertyKey PKEY_Device_CompatibleIds = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 4); // STRING_LIST
            public static PropertyKey PKEY_Device_Service = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 6); // STRING
            public static PropertyKey PKEY_Device_Class = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 9); // STRING
            public static PropertyKey PKEY_Device_ClassGuid = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 10); // GUID
            public static PropertyKey PKEY_Device_Driver = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 11); // STRING
            public static PropertyKey PKEY_Device_ConfigFlags = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 12); // UINT32
            public static PropertyKey PKEY_Device_Manufacturer = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 13); // STRING
            public static PropertyKey PKEY_Device_LocationInfo = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 15); // STRING
            public static PropertyKey PKEY_Device_PDOName = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 16); // STRING
            public static PropertyKey PKEY_Device_Capabilities = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 17); // UINT32
            public static PropertyKey PKEY_Device_UINumber = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 18); // UINT32
            public static PropertyKey PKEY_Device_UpperFilters = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 19); // STRING_LIST
            public static PropertyKey PKEY_Device_LowerFilters = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 20); // STRING_LIST
            public static PropertyKey PKEY_Device_BusTypeGuid = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 21); // GUID
            public static PropertyKey PKEY_Device_LegacyBusType = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 22); // UINT32
            public static PropertyKey PKEY_Device_BusNumber = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 23); // UINT32
            public static PropertyKey PKEY_Device_EnumeratorName = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 24); // STRING
            public static PropertyKey PKEY_Device_Security = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 25); // SECURITY_DESCRIPTOR
            public static PropertyKey PKEY_Device_SecuritySDS = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 26); // SECURITY_DESCRIPTOR_STRING
            public static PropertyKey PKEY_Device_DevType = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 27); // UINT32
            public static PropertyKey PKEY_Device_Exclusive = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 28); // BOOLEAN
            public static PropertyKey PKEY_Device_Characteristics = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 29); // UINT32
            public static PropertyKey PKEY_Device_Address = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 30); // UINT32
            public static PropertyKey PKEY_Device_UINumberDescFormat = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 31); // STRING
            public static PropertyKey PKEY_Device_PowerData = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 32); // BINARY
            public static PropertyKey PKEY_Device_RemovalPolicy = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 33); // UINT32
            public static PropertyKey PKEY_Device_RemovalPolicyDefault = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 34); // UINT32
            public static PropertyKey PKEY_Device_RemovalPolicyOverride = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 35); // UINT32
            public static PropertyKey PKEY_Device_InstallState = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 36); // UINT32
            public static PropertyKey PKEY_Device_LocationPaths = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 37); // STRING_LIST
            public static PropertyKey PKEY_Device_BaseContainerId = new(new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), 38); // GUID
            public static PropertyKey PKEY_Device_Model = new(new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57), 39); // STRING
        }

        /// <summary>For <see cref="PKeys.PKEY_AudioEndpoint_FormFactor"/>.</summary>
        public enum EndpointFormFactor : int
        {
            RemoteNetworkDevice, // = 0
            Speakers,
            LineLevel,
            Headphones,
            Microphone,
            Headset,
            Handset,
            UnknownDigitalPassthrough,
            SPDIF,
            DigitalAudioDisplayDevice,
            UnknownFormFactor
        }

        /// <summary>Enum of VARTYPE for a PropertyVariant.</summary>
        public enum VarType
        {
            VT_EMPTY = 0, VT_NULL = 1, VT_I2 = 2, VT_I4 = 3, VT_R4 = 4, VT_R8 = 5, VT_CY = 6, VT_DATE = 7, VT_BSTR = 8,
            VT_DISPATCH = 9, VT_ERROR = 10, VT_BOOL = 11, VT_VARIANT = 12, VT_UNKNOWN = 13, VT_DECIMAL = 14, VT_I1 = 16,
            VT_UI1 = 17, VT_UI2 = 18, VT_UI4 = 19, VT_I8 = 20, VT_UI8 = 21, VT_INT = 22, VT_UINT = 23, VT_VOID = 24,
            VT_HRESULT = 25, VT_PTR = 26, VT_SAFEARRAY = 27, VT_CARRAY = 28, VT_USERDEFINED = 29, VT_LPSTR = 30, VT_LPWSTR = 31,
            VT_RECORD = 36, VT_INT_PTR = 37, VT_UINT_PTR = 38, VT_FILETIME = 64, VT_BLOB = 65, VT_STREAM = 66, VT_STORAGE = 67,
            VT_STREAMED_OBJECT = 68, VT_STORED_OBJECT = 69, VT_BLOB_OBJECT = 70, VT_CF = 71, VT_CLSID = 72, VT_VERSIONED_STREAM = 73,
            VT_BSTR_BLOB = 0xfff, VT_VECTOR = 0x1000, VT_ARRAY = 0x2000, VT_BYREF = 0x4000, VT_RESERVED = 0x8000, VT_ILLEGAL = 0xffff
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>The IMMDeviceEnumerator interface provides methods for enumerating multimedia device resources. In the current implementation of the MMDevice API, the only device resources that this interface can enumerate are audio endpoint devices. A client obtains a reference to an IMMDeviceEnumerator interface by calling the CoCreateInstance function, as described previously (see MMDevice API).</summary>
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMMDeviceEnumerator
        {
            /// <summary>The EnumAudioEndpoints method generates a collection of audio endpoint devices that meet the specified criteria.</summary>
            /// <param name="dataFlow">The data-flow direction for the endpoint devices in the collection.</param>
            /// <param name="dwStateMask">The state or states of the endpoints that are to be included in the collection. </param>
            /// <param name="ppDevices">Pointer to a pointer variable into which the method writes the address of the IMMDeviceCollection interface of the device-collection object. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the EnumAudioEndpoints call fails, *ppDevices is NULL.</param>
            public int EnumAudioEndpoints(int dataFlow, int dwStateMask, out nint ppDevices);

            /// <summary>The GetDefaultAudioEndpoint method retrieves the default audio endpoint for the specified data-flow direction and role.</summary>
            /// <param name="dataFlow">The data-flow direction for the endpoint device.</param>
            /// <param name="role">The role of the endpoint device.</param>
            /// <param name="ppEndpoint">Pointer to a pointer variable into which the method writes the address of the IMMDevice interface of the endpoint object for the default audio endpoint device. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the GetDefaultAudioEndpoint call fails, *ppDevice is NULL.</param>
            public int GetDefaultAudioEndpoint(int dataFlow, int role, out nint ppEndpoint);

            /// <summary>The GetDevice method retrieves an audio endpoint device that is identified by an endpoint ID string.</summary>
            /// <param name="pwstrId">Pointer to a string containing the endpoint ID. The caller typically obtains this string from the IMMDevice::GetId method or from one of the methods in the IMMNotificationClient interface.</param>
            /// <param name="ppDevice">Pointer to a pointer variable into which the method writes the address of the IMMDevice interface for the specified device. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the GetDevice call fails, *ppDevice is NULL.</param>
            public int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out nint ppDevice);

            /// <summary>The RegisterEndpointNotificationCallback method registers a client's notification callback interface.</summary>
            /// <param name="pClient">Pointer to the IMMNotificationClient interface that the client is registering for notification callbacks.</param>
            public int RegisterEndpointNotificationCallback(nint pClient);
        }

        /// <summary>The IMMDeviceCollection interface represents a collection of multimedia device resources. In the current implementation, the only device resources that the MMDevice API can create collections of are audio endpoint devices.</summary>
        [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMMDeviceCollection
        {
            /// <summary>The GetCount method retrieves a count of the devices in the device collection.</summary>
            /// <param name="pcDevices">A UINT variable into which the method writes the number of devices in the device collection.</param>
            public int GetCount(out uint pcDevices);

            /// <summary>The Item method retrieves a pointer to the specified item in the device collection.</summary>
            /// <param name="nDevice">The device number. If the collection contains n devices, the devices are numbered 0 to n– 1.</param>
            /// <param name="ppDevice">Pointer to a pointer variable into which the method writes the address of the IMMDevice interface of the specified item in the device collection. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the Item call fails, *ppDevice is NULL.</param>
            public int Item(uint nDevice, out nint ppDevice);
        }

        /// <summary>The IMMDevice interface encapsulates the generic features of a multimedia device resource. In the current implementation of the MMDevice API, the only type of device resource that an IMMDevice interface can represent is an audio endpoint device.</summary>
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IMMDevice
        {
            /// <summary>The Activate method creates a COM object with the specified interface.</summary>
            /// <param name="iid">The interface identifier. This parameter is a reference to a GUID that identifies the interface that the caller requests be activated. The caller will use this interface to communicate with the COM object.</param>
            /// <param name="dwClsCtx">The execution context in which the code that manages the newly created object will run. The caller can restrict the context by setting this parameter to the bitwise OR of one or more CLSCTX enumeration values. Alternatively, the client can avoid imposing any context restrictions by specifying CLSCTX_ALL. For more information about CLSCTX, see the Windows SDK documentation.</param>
            /// <param name="pActivationParams">Set to NULL to activate an IAudioClient, IAudioEndpointVolume, IAudioMeterInformation, IAudioSessionManager, or IDeviceTopology interface on an audio endpoint device. When activating an IBaseFilter, IDirectSound, IDirectSound8, IDirectSoundCapture, or IDirectSoundCapture8 interface on the device, the caller can specify a pointer to a PROPVARIANT structure that contains stream-initialization information. For more information, see Remarks.</param>
            /// <param name="ppInterface">Pointer to a pointer variable into which the method writes the address of the interface specified by parameter iid. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the Activate call fails, *ppInterface is NULL.</param>
            public int Activate(ref Guid iid, uint dwClsCtx, nint pActivationParams, out nint ppInterface);

            /// <summary>The OpenPropertyStore method retrieves an interface to the device's property store.</summary>
            /// <param name="stgmAccess">The storage-access mode. This parameter specifies whether to open the property store in read mode, write mode, or read/write mode. The method permits a client running as an administrator to open a store for read-only, write-only, or read/write access. A client that is not running as an administrator is restricted to read-only access.</param>
            /// <param name="ppProperties">Pointer to a pointer variable into which the method writes the address of the IPropertyStore interface of the device's property store. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the OpenPropertyStore call fails, *ppProperties is NULL. For more information about IPropertyStore, see the Windows SDK documentation.</param>
            public int OpenPropertyStore(int stgmAccess, out nint ppProperties);

            /// <summary>The GetId method retrieves an endpoint ID string that identifies the audio endpoint device.</summary>
            /// <param name="ppstrId">Pointer to a pointer variable into which the method writes the address of a null-terminated, wide-character string containing the endpoint device ID. The method allocates the storage for the string. The caller is responsible for freeing the storage, when it is no longer needed, by calling the CoTaskMemFree function. If the GetId call fails, *ppstrId is NULL. For information about CoTaskMemFree, see the Windows SDK documentation.</param>
            public int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);

            /// <summary>The GetState method retrieves the current device state.</summary>
            /// <param name="pdwState">Pointer to a DWORD variable into which the method writes the current state of the device.</param>
            public int GetState(out int pdwState);
        }

        /// <summary>The IAudioClient interface enables a client to create and initialize an audio stream between an audio application and the audio engine (for a shared-mode stream) or the hardware buffer of an audio endpoint device (for an exclusive-mode stream). </summary>
        [Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioClient
        {
            /// <summary>The Initialize method initializes the audio stream.</summary>
            /// <param name="shareMode">The sharing mode for the connection. Through this parameter, the client tells the audio engine whether it wants to share the audio endpoint device with other clients.</param>
            /// <param name="streamFlags">Flags to control creation of the stream. The client should set this parameter to 0 or to the bitwise OR of one or more of the AUDCLNT_STREAMFLAGS_XXX Constants or the AUDCLNT_SESSIONFLAGS_XXX Constants.</param>
            /// <param name="hnsBufferDuration">The buffer capacity as a time value. This parameter is of type REFERENCE_TIME and is expressed in 100-nanosecond units. This parameter contains the buffer size that the caller requests for the buffer that the audio application will share with the audio engine (in shared mode) or with the endpoint device (in exclusive mode). If the call succeeds, the method allocates a buffer that is a least this large. For more information about REFERENCE_TIME, see the Windows SDK documentation. For more information about buffering requirements, see Remarks.</param>
            /// <param name="hnsPeriodicity">The device period. This parameter can be nonzero only in exclusive mode. In shared mode, always set this parameter to 0. In exclusive mode, this parameter specifies the requested scheduling period for successive buffer accesses by the audio endpoint device. If the requested device period lies outside the range that is set by the device's minimum period and the system's maximum period, then the method clamps the period to that range. If this parameter is 0, the method sets the device period to its default value. To obtain the default device period, call the IAudioClient::GetDevicePeriod method. If the AUDCLNT_STREAMFLAGS_EVENTCALLBACK stream flag is set and AUDCLNT_SHAREMODE_EXCLUSIVE is set as the ShareMode, then hnsPeriodicity must be nonzero and equal to hnsBufferDuration.</param>
            /// <param name="pFormat">Pointer to a format descriptor. This parameter must point to a valid format descriptor of type WAVEFORMATEX (or WAVEFORMATEXTENSIBLE). For more information, see Remarks.</param>
            /// <param name="audioSessionGuid">Pointer to a session GUID. This parameter points to a GUID value that identifies the audio session that the stream belongs to. If the GUID identifies a session that has been previously opened, the method adds the stream to that session. If the GUID does not identify an existing session, the method opens a new session and adds the stream to that session. The stream remains a member of the same session for its lifetime. Setting this parameter to NULL is equivalent to passing a pointer to a GUID_NULL value.</param>
            public int Initialize(int shareMode, uint streamFlags, long hnsBufferDuration, long hnsPeriodicity, [In] ref WaveFormatEx pFormat, nint audioSessionGuid);

            /// <summary>The GetBufferSize method retrieves the size (maximum capacity) of the endpoint buffer.</summary>
            /// <param name="numBufferFrames">Pointer to a UINT32 variable into which the method writes the number of audio frames that the buffer can hold.</param>
            public int GetBufferSize(out uint numBufferFrames);

            /// <summary>The GetStreamLatency method retrieves the maximum latency for the current stream and can be called any time after the stream has been initialized.</summary>
            /// <param name="hnsLatency">Pointer to a REFERENCE_TIME variable into which the method writes a time value representing the latency. The time is expressed in 100-nanosecond units. For more information about REFERENCE_TIME, see the Windows SDK documentation.</param>
            public int GetStreamLatency(out long hnsLatency);

            /// <summary>The GetCurrentPadding method retrieves the number of frames of padding in the endpoint buffer.</summary>
            /// <param name="numPaddingFrames">Pointer to a UINT32 variable into which the method writes the frame count (the number of audio frames of padding in the buffer).</param>
            public int GetCurrentPadding(out uint numPaddingFrames);

            /// <summary>The IsFormatSupported method indicates whether the audio endpoint device supports a particular stream format.</summary>
            /// <param name="shareMode">The sharing mode for the stream format. Through this parameter, the client indicates whether it wants to use the specified format in exclusive mode or shared mode.</param>
            /// <param name="pFormat">Pointer to the specified stream format. This parameter points to a caller-allocated format descriptor of type WAVEFORMATEX or WAVEFORMATEXTENSIBLE. The client writes a format description to this structure before calling this method. For information about WAVEFORMATEX and WAVEFORMATEXTENSIBLE, see the Windows DDK documentation.</param>
            /// <param name="closestMatch">Pointer to a pointer variable into which the method writes the address of a WAVEFORMATEX or WAVEFORMATEXTENSIBLE structure. This structure specifies the supported format that is closest to the format that the client specified through the pFormat parameter. For shared mode (that is, if the ShareMode parameter is AUDCLNT_SHAREMODE_SHARED), set ppClosestMatch to point to a valid, non-NULL pointer variable. For exclusive mode, set ppClosestMatch to NULL. The method allocates the storage for the structure. The caller is responsible for freeing the storage, when it is no longer needed, by calling the CoTaskMemFree function. If the IsFormatSupported call fails and ppClosestMatch is non-NULL, the method sets *ppClosestMatch to NULL. For information about CoTaskMemFree, see the Windows SDK documentation.</param>
            public int IsFormatSupported(int shareMode, [In] ref WaveFormatEx pFormat, out nint closestMatch);

            /// <summary>The GetMixFormat method retrieves the stream format that the audio engine uses for its internal processing of shared-mode streams.</summary>
            /// <param name="ppDeviceFormat">Pointer to a pointer variable into which the method writes the address of the mix format. This parameter must be a valid, non-NULL pointer to a pointer variable. The method writes the address of a WAVEFORMATEX (or WAVEFORMATEXTENSIBLE) structure to this variable. The method allocates the storage for the structure. The caller is responsible for freeing the storage, when it is no longer needed, by calling the CoTaskMemFree function. If the GetMixFormat call fails, *ppDeviceFormat is NULL. For information about WAVEFORMATEX, WAVEFORMATEXTENSIBLE, and CoTaskMemFree, see the Windows SDK documentation.</param>
            public int GetMixFormat(out nint ppDeviceFormat);

            /// <summary>The GetDevicePeriod method retrieves the length of the periodic interval separating successive processing passes by the audio engine on the data in the endpoint buffer.</summary>
            /// <param name="hnsDefaultDevicePeriod">Pointer to a REFERENCE_TIME variable into which the method writes a time value specifying the default interval between periodic processing passes by the audio engine. The time is expressed in 100-nanosecond units. For information about REFERENCE_TIME, see the Windows SDK documentation.</param>
            /// <param name="hnsMinimumDevicePeriod">Pointer to a REFERENCE_TIME variable into which the method writes a time value specifying the minimum interval between periodic processing passes by the audio endpoint device. The time is expressed in 100-nanosecond units.</param>
            public int GetDevicePeriod(out long hnsDefaultDevicePeriod, out long hnsMinimumDevicePeriod);

            /// <summary>The Start method starts the audio stream.</summary>
            public int Start();

            /// <summary>The Stop method stops the audio stream.</summary>
            public int Stop();

            /// <summary>The Reset method resets the audio stream.</summary>
            public int Reset();

            /// <summary>The SetEventHandle method sets the event handle that the system signals when an audio buffer is ready to be processed by the client.</summary>
            /// <param name="eventHandle">The event handle.</param>
            public int SetEventHandle(nint eventHandle);

            /// <summary>The GetService method accesses additional services from the audio client object.</summary>
            /// <param name="riid">The interface ID for the requested service.</param>
            /// <param name="ppv">Pointer to a pointer variable into which the method writes the address of an instance of the requested interface. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the GetService call fails, *ppv is NULL.</param>
            public int GetService(ref Guid riid, out nint ppv);
        }

        /// <summary>The IAudioRenderClient interface enables a client to write output data to a rendering endpoint buffer. The client obtains a reference to the IAudioRenderClient interface of a stream object by calling the IAudioClient::GetService method with parameter riid set to REFIID IID_IAudioRenderClient.
        /// The methods in this interface manage the movement of data packets that contain audio-rendering data.The length of a data packet is expressed as the number of audio frames in the packet. The size of an audio frame is specified by the nBlockAlign member of the WAVEFORMATEX structure that the client obtains by calling the IAudioClient::GetMixFormat method. The size in bytes of an audio frame equals the number of channels in the stream multiplied by the sample size per channel.For example, the frame size is four bytes for a stereo (2-channel) stream with 16-bit samples.A packet always contains an integral number of audio frames.
        /// When releasing an IAudioRenderClient interface instance, the client must call the interface's Release method from the same thread as the call to IAudioClient::GetService that created the object.</summary>
        [Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAudioRenderClient
        {
            /// <summary>Retrieves a pointer to the next available space in the rendering endpoint buffer into which the caller can write a data packet.</summary>
            /// <param name="numFramesRequested">The number of audio frames in the data packet that the caller plans to write to the requested space in the buffer. If the call succeeds, the size of the buffer area pointed to by *ppData matches the size specified in NumFramesRequested.</param>
            /// <param name="ppData">Pointer to a pointer variable into which the method writes the starting address of the buffer area into which the caller will write the data packet.</param>
            public int GetBuffer(uint numFramesRequested, out nint ppData);

            /// <summary>The ReleaseBuffer method releases the buffer space acquired in the previous call to the IAudioRenderClient::GetBuffer method.</summary>
            /// <param name="numFramesWritten">The number of audio frames written by the client to the data packet. The value of this parameter must be less than or equal to the size of the data packet, as specified in the NumFramesRequested parameter passed to the IAudioRenderClient::GetBuffer method.</param>
            /// <param name="dwFlags">The buffer-configuration flags.</param>
            public int ReleaseBuffer(uint numFramesWritten, uint dwFlags);
        }

        /// <summary>The WAVEFORMATEX structure defines the format of waveform-audio data. Only format information common to all waveform-audio data formats is included in this structure. For formats that require additional information, this structure is included as the first member in another structure, along with the additional information.</summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WaveFormatEx
        {
            /// <summary>Waveform-audio format type. Format tags are registered with Microsoft Corporation for many compression algorithms. A complete list of format tags can be found in the Mmreg.h header file. For one- or two-channel PCM data, this value should be WAVE_FORMAT_PCM. When this structure is included in a WAVEFORMATEXTENSIBLE structure, this value must be WAVE_FORMAT_EXTENSIBLE.</summary>
            public ushort wFormatTag;

            /// <summary>Number of channels in the waveform-audio data. Monaural data uses one channel and stereo data uses two channels.</summary>
            public ushort nChannels;

            /// <summary>Sample rate, in samples per second (hertz). If wFormatTag is WAVE_FORMAT_PCM, then common values for nSamplesPerSec are 8.0 kHz, 11.025 kHz, 22.05 kHz, and 44.1 kHz. For non-PCM formats, this member must be computed according to the manufacturer's specification of the format tag.</summary>
            public uint nSamplesPerSec;

            /// <summary>Required average data-transfer rate, in bytes per second, for the format tag. If wFormatTag is WAVE_FORMAT_PCM, nAvgBytesPerSec should be equal to the product of nSamplesPerSec and nBlockAlign. For non-PCM formats, this member must be computed according to the manufacturer's specification of the format tag.</summary>
            public uint nAvgBytesPerSec;

            /// <summary>Block alignment, in bytes. The block alignment is the minimum atomic unit of data for the wFormatTag format type. If wFormatTag is WAVE_FORMAT_PCM or WAVE_FORMAT_EXTENSIBLE, nBlockAlign must be equal to the product of nChannels and wBitsPerSample divided by 8 (bits per byte). For non-PCM formats, this member must be computed according to the manufacturer's specification of the format tag.</summary>
            public ushort nBlockAlign;

            /// <summary>Bits per sample for the wFormatTag format type. If wFormatTag is WAVE_FORMAT_PCM, then wBitsPerSample should be equal to 8 or 16. For non-PCM formats, this member must be set according to the manufacturer's specification of the format tag. If wFormatTag is WAVE_FORMAT_EXTENSIBLE, this value can be any integer multiple of 8 and represents the container size, not necessarily the sample size; for example, a 20-bit sample size is in a 24-bit container. Some compression schemes cannot define a value for wBitsPerSample, so this member can be 0.</summary>
            public ushort wBitsPerSample;

            /// <summary>Size, in bytes, of extra format information appended to the end of the WAVEFORMATEX structure. This information can be used by non-PCM formats to store extra attributes for the wFormatTag. If no extra information is required by the wFormatTag, this member must be set to 0. For WAVE_FORMAT_PCM formats (and only WAVE_FORMAT_PCM formats), this member is ignored. When this structure is included in a WAVEFORMATEXTENSIBLE structure, this value must be at least 22.</summary>
            public ushort cbSize;

            /// <summary>FGE Helper to construct an instance properly.</summary>
            /// <param name="rate">Sample rate.</param>
            /// <param name="channels">Number of channels.</param>
            /// <param name="bytesPerSample">How many bytes in each sample.</param>
            public static WaveFormatEx Create(int rate = 44100, int channels = 2, int bytesPerSample = 2)
            {
                return new WaveFormatEx
                {
                    wFormatTag = 1, // WAVE_FORMAT_PCM
                    nChannels = (ushort)channels,
                    nSamplesPerSec = (uint)rate,
                    wBitsPerSample = (ushort)(bytesPerSample * 8),
                    cbSize = 0,
                    nBlockAlign = (ushort)(channels * bytesPerSample),
                    nAvgBytesPerSec = (uint)(rate * channels * bytesPerSample)
                };
            }
        }
    }

    /// <summary>Raw internal data for the WASAPI handler.</summary>
    public struct InternalData
    {
        /// <summary>Internal raw references.</summary>
        public nint RefAudioClient, RefEnumerator, RefRenderClient;

        /// <summary>The WASAPI audio client.</summary>
        public Native.IAudioClient AudioClient;

        /// <summary>The WASAPI render client.</summary>
        public Native.IAudioRenderClient RenderClient;

        /// <summary>The WASAPI device enumerator.</summary>
        public Native.IMMDeviceEnumerator Enumerator;

        /// <summary>Raw wave format data for WASAPI init.</summary>
        public Native.WaveFormatEx Format;

        /// <summary>Count of frames allowed in the WASAPI buffer.</summary>
        public uint BufferFrameCount;

        /// <summary>Number of "Reference Times" in one millisecond. One "Reference Time" is 100 nanoseconds.</summary>
        public const int REFTIMES_PER_MS = 10000;

        /// <summary>Class ID for <see cref="Native.IMMDeviceEnumerator"/>.</summary>
        public static Guid CLSID_MMDeviceEnumerator = new("BCDE0395-E52F-467C-8E3D-C4579291692E");

        /// <summary>Ref-Interface ID for <see cref="Native.IMMDeviceEnumerator"/>.</summary>
        public static Guid IID_IMMDeviceEnumerator = new("A95664D2-9614-4F35-A746-DE8DB63617E6");

        /// <summary>Ref-Interface ID for <see cref="Native.IAudioClient"/>.</summary>
        public static Guid IID_IAudioClient = new("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");

        /// <summary>Ref-Interface ID for <see cref="Native.IAudioRenderClient"/>.</summary>
        public static Guid IID_IAudioRenderClient = new("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");

        /// <summary>Reusable internal raw binary buffers.</summary>
        public Queue<byte[]> RawDataBuffers;

        /// <summary>Converts an HResult into an exception if needed.</summary>
        public static void CheckHResult(int hr, string operation)
        {
            if (hr != 0 && hr != 1)
            {
                throw new Exception($"WASAPI {operation} failed with error code: 0x{hr:X8}");
            }
        }

        /// <summary>Equivalent to <see cref="Marshal.GetObjectForNativeVariant"/> but actually works for strings and stuff.</summary>
        public static object GetObjectFromPropVariant(nint pv)
        {
            Native.VarType varType = (Native.VarType)Marshal.ReadInt16(pv);
            return varType switch
            {
                Native.VarType.VT_EMPTY or Native.VarType.VT_NULL or Native.VarType.VT_VOID => null,
                Native.VarType.VT_LPSTR => Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(pv + 8)),
                Native.VarType.VT_BSTR => Marshal.PtrToStringBSTR(Marshal.ReadIntPtr(pv + 8)),
                Native.VarType.VT_LPWSTR => Marshal.PtrToStringUni(Marshal.ReadIntPtr(pv + 8)),
                Native.VarType.VT_FILETIME => DateTime.FromFileTimeUtc(Marshal.ReadInt64(pv + 8)),
                Native.VarType.VT_CLSID => Marshal.PtrToStructure<Guid>(Marshal.ReadIntPtr(pv + 8)),
                Native.VarType.VT_I1 or Native.VarType.VT_UI1 or Native.VarType.VT_I2 or Native.VarType.VT_I4 or Native.VarType.VT_I8
                or Native.VarType.VT_UI2 or Native.VarType.VT_UI4 or Native.VarType.VT_UI8 or Native.VarType.VT_R4 or Native.VarType.VT_R8
                or Native.VarType.VT_BOOL or Native.VarType.VT_DECIMAL or Native.VarType.VT_INT or Native.VarType.VT_UINT
                or Native.VarType.VT_ERROR or Native.VarType.VT_HRESULT => Marshal.GetObjectForNativeVariant(pv),
                _ => throw new NotSupportedException($"Unsupported PropVariant type: {varType}"),
            };
        }

        /// <summary>Gets a value from a property store, and converts it to a C# type.</summary>
        public static object GetPropertyValueFixed(Native.IPropertyStore propStore, Native.PropertyKey key)
        {
            nint pv = Marshal.AllocCoTaskMem(32);
            CheckHResult(propStore.GetValue(ref key, pv), "propertyStore.GetValue");
            object obj = GetObjectFromPropVariant(pv);
            CheckHResult(Native.PropVariantClear(pv), "PropVariantClear");
            Marshal.FreeCoTaskMem(pv);
            return obj;
        }
    }

    /// <summary>Raw internal data for the WASAPI handler.</summary>
    public InternalData Internal = new() { RawDataBuffers = [] };

    /// <inheritdoc/>
    public override List<AudioDevice> ListAllAudioDevices()
    {
        List<AudioDevice> result = [];
        InternalData.CheckHResult(Internal.Enumerator.EnumAudioEndpoints(0 /* eRender */, 1 /* DEVICE_STATE_ACTIVE */, out nint ppDevices), "GetDefaultAudioEndpoint");
        Native.IMMDeviceCollection collection = (Native.IMMDeviceCollection)Marshal.GetTypedObjectForIUnknown(ppDevices, typeof(Native.IMMDeviceCollection));
        InternalData.CheckHResult(collection.GetCount(out uint deviceCount), "collection.GetCount");
        for (uint i = 0; i < deviceCount; i++)
        {
            InternalData.CheckHResult(collection.Item(i, out nint ppDevice), "collection.Item");
            Native.IMMDevice device = (Native.IMMDevice)Marshal.GetTypedObjectForIUnknown(ppDevice, typeof(Native.IMMDevice));
            InternalData.CheckHResult(device.GetId(out string deviceId), "device.GetId");
            InternalData.CheckHResult(device.OpenPropertyStore((int)Native.STGMConstants.READ, out nint ppProperties), "device.OpenPropertyStore");
            Native.IPropertyStore propertyStore = (Native.IPropertyStore)Marshal.GetTypedObjectForIUnknown(ppProperties, typeof(Native.IPropertyStore));
            string friendlyName = $"{InternalData.GetPropertyValueFixed(propertyStore, Native.PKeys.PKEY_Device_FriendlyName)}";
            string description = $"{InternalData.GetPropertyValueFixed(propertyStore, Native.PKeys.PKEY_Device_DeviceDesc)}";
            string guid = $"{InternalData.GetPropertyValueFixed(propertyStore, Native.PKeys.PKEY_AudioEndpoint_GUID)}";
            uint formFactorIndex = (uint)InternalData.GetPropertyValueFixed(propertyStore, Native.PKeys.PKEY_AudioEndpoint_FormFactor);
            Native.EndpointFormFactor formFactor = (Native.EndpointFormFactor)formFactorIndex;
            AudioDevice deviceOut = new()
            {
                Name = friendlyName,
                UID = guid,
                InternalID = deviceId,
                FullDescriptionText = $"{friendlyName}:\n  Description: {description}\n  Form Factor: {formFactor}\n  GUID: {guid}\n  WASAPI ID: {deviceId}\n  Index: {i}",
                IDs = [guid, deviceId, friendlyName, description, $"{formFactor}", $"{i}"]
            };
            Marshal.ReleaseComObject(device);
            result.Add(deviceOut);
        }
        Marshal.ReleaseComObject(collection);
        return result;
    }

    /// <inheritdoc/>
    public override void PreInit()
    {
        // TODO: Is COM initialize redundant in C# context? Doesn't hurt to have though.
        InternalData.CheckHResult(Native.CoInitializeEx(IntPtr.Zero, (uint)Native.COINIT.MULTITHREADED), "CoInitializeEx");
        InternalData.CheckHResult(Native.CoCreateInstance(ref InternalData.CLSID_MMDeviceEnumerator, IntPtr.Zero, Native.CLSCTX_ALL, ref InternalData.IID_IMMDeviceEnumerator, out Internal.RefEnumerator), "CoCreateInstance");
        if (Internal.RefEnumerator == nint.Zero)
        {
            throw new Exception("Failed to create IMMDeviceEnumerator");
        }
        Internal.Enumerator = (Native.IMMDeviceEnumerator)Marshal.GetTypedObjectForIUnknown(Internal.RefEnumerator, typeof(Native.IMMDeviceEnumerator));
    }

    /// <inheritdoc/>
    public override void SelectDeviceAndInit(AudioDevice device)
    {
        if (Internal.AudioClient is not null)
        {
            Shutdown();
            PreInit();
        }
        nint refDevice = 0;
        if (device is not null)
        {
            InternalData.CheckHResult(Internal.Enumerator.GetDevice(device.InternalID, out refDevice), "GetDefaultAudioEndpoint");
        }
        if (refDevice == nint.Zero)
        {
            InternalData.CheckHResult(Internal.Enumerator.GetDefaultAudioEndpoint(0 /* eRender */, 0 /* eConsole */, out refDevice), "GetDefaultAudioEndpoint");
            if (refDevice == nint.Zero)
            {
                throw new Exception("Failed to get default IMMDevice");
            }
        }
        Native.IMMDevice mmDevice = (Native.IMMDevice)Marshal.GetTypedObjectForIUnknown(refDevice, typeof(Native.IMMDevice));
        InternalData.CheckHResult(mmDevice.Activate(ref InternalData.IID_IAudioClient, Native.CLSCTX_ALL, IntPtr.Zero, out Internal.RefAudioClient), "device.Activate");
        Internal.AudioClient = (Native.IAudioClient)Marshal.GetTypedObjectForIUnknown(Internal.RefAudioClient, typeof(Native.IAudioClient));
        // TODO: Dynamic format based on device config / user settings
        Internal.Format = Native.WaveFormatEx.Create(FGE3DAudioEngine.InternalData.FREQUENCY, 2);
        uint streamFlags = (uint)(Native.AUDCLNT_STREAMFLAGS.AUTOCONVERTPCM | Native.AUDCLNT_STREAMFLAGS.SRC_DEFAULT_QUALITY);
        InternalData.CheckHResult(Internal.AudioClient.Initialize((int)Native.AUDCLNT_SHAREMODE.SHARED, streamFlags, InternalData.REFTIMES_PER_MS * 1000, 0, ref Internal.Format, nint.Zero), "audioClient.Initialize");
        InternalData.CheckHResult(Internal.AudioClient.GetBufferSize(out Internal.BufferFrameCount), "audioClient.GetBufferSize");
        InternalData.CheckHResult(Internal.AudioClient.GetService(ref InternalData.IID_IAudioRenderClient, out Internal.RefRenderClient), "audioClient.GetService");
        if (Internal.RefRenderClient == nint.Zero)
        {
            throw new Exception("Failed to get IAudioRenderClient");
        }
        Internal.RenderClient = (Native.IAudioRenderClient)Marshal.GetTypedObjectForIUnknown(Internal.RefRenderClient, typeof(Native.IAudioRenderClient));
        Internal.AudioClient.Reset();
        Internal.AudioClient.Start();
        Marshal.ReleaseComObject(mmDevice);
        Logs.ClientInit($"Audio system initialized using WASAPI... device selection: {device?.Name ?? "default"}");
    }

    /// <inheritdoc/>
    public override void Shutdown()
    {
        Internal.AudioClient.Stop();
        Marshal.ReleaseComObject(Internal.AudioClient);
        Marshal.ReleaseComObject(Internal.Enumerator);
        Marshal.ReleaseComObject(Internal.RenderClient);
        Internal.AudioClient = null;
        Internal.RenderClient = null;
        Internal.Enumerator = null;
        Internal.RefAudioClient = nint.Zero;
        Internal.RefEnumerator = nint.Zero;
        Internal.RefRenderClient = nint.Zero;
    }

    /// <inheritdoc/>
    public override bool PreprocessStep()
    {
        // padding is the number of frames queued but not yet played
        Internal.AudioClient.GetCurrentPadding(out uint padding);
        uint availableFrames = Internal.BufferFrameCount - padding;
        return availableFrames > FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER && padding < FGE3DAudioEngine.InternalData.BUFFERS_AT_ONCE * FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER;
    }

    /// <inheritdoc/>
    public override unsafe void SendNextBuffer(FGE3DAudioEngine engine)
    {
        // 4 = 2 bytes per sample, 2 channels
        byte[] rawBuffer = Internal.RawDataBuffers.Count > 0 ? Internal.RawDataBuffers.Dequeue() : new byte[FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER * 4];
        AudioChannel leftChannel = engine.Channels.FirstOrDefault(c => c.Name == "Left");
        AudioChannel rightChannel = engine.Channels.FirstOrDefault(c => c.Name == "Right");
        short* leftBuffer = leftChannel.InternalCurrentBuffer;
        short* rightBuffer = rightChannel.InternalCurrentBuffer;
        GraphicsUtil.DebugAssert(leftBuffer != null && rightBuffer != null, "Left and right buffers must be non-null.");
        fixed (byte* rawBufferPtr = rawBuffer)
        {
            for (int i = 0; i < FGE3DAudioEngine.InternalData.SAMPLES_PER_BUFFER; i++)
            {
                short left = leftBuffer[i], right = rightBuffer[i];
                byte* subPtr = rawBufferPtr + (i * 4);
                subPtr[0] = unchecked((byte)(left & 0xff));
                subPtr[1] = unchecked((byte)((left >> 8) & 0xff));
                subPtr[2] = unchecked((byte)(right & 0xff));
                subPtr[3] = unchecked((byte)((right >> 8) & 0xff));
            }
        }
        int bytesPerFrame = Internal.Format.nBlockAlign;
        uint frames = (uint)(rawBuffer.Length / bytesPerFrame);
        Internal.AudioClient.GetCurrentPadding(out uint padding);
        uint availableFrames = Internal.BufferFrameCount - padding;
        Internal.RenderClient.GetBuffer(frames, out nint pData);
        int bytesToWrite = (int)(frames * bytesPerFrame);
        Marshal.Copy(rawBuffer, 0, pData, bytesToWrite);
        Internal.RenderClient.ReleaseBuffer(frames, 0);
    }

    /// <inheritdoc/>
    public override void MakeCurrent()
    {
        // Not needed
    }
}
