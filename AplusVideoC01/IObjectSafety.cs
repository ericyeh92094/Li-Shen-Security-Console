using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AplusVideoC01
{
    /// <summary>

    /// 針對 ActiveX Control 的 IObjectSafety 介面引用。

    /// </summary>

    [ComImport]

    [Guid("DA3EBCFA-52D1-4783-A442-CE14DC7C3873")]

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

    public interface IObjectSafety
    {

        /// <summary>

        /// 取得物件的安全選項。

        /// </summary>

        /// <param name="riid">介面代碼。</param>

        /// <param name="pdwSupportedOptions">支援的安全性選項。</param>

        /// <param name="pdwEnabledOptions">啟用選項。</param>

        void GetInterfaceSafetyOptions(int riid, out int pdwSupportedOptions, out int pdwEnabledOptions);

        /// <summary>

        /// 設定物件的安全選項。

        /// </summary>

        /// <param name="riid">介面代碼。</param>

        /// <param name="pdwSupportedOptions">支援的安全性選項。</param>

        /// <param name="pdwEnabledOptions">啟用選項。</param>

        void SetInterfaceSafetyOptions(int riid, int pdwSupportedOptions, int pdwEnabledOptions);

    }

    /// <summary>

    /// IObjectSafety 使用的列舉值。

    /// </summary>

    public class IObjectSafetyEnums
    {

        /// <summary>

        /// 對未受信任的呼叫者宣布此介面為安全。

        /// </summary>

        public const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x0000001;

        /// <summary>

        /// 對未受信任的資料宣布此介面為安全。

        /// </summary>

        public const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x0000002;

    }
}
