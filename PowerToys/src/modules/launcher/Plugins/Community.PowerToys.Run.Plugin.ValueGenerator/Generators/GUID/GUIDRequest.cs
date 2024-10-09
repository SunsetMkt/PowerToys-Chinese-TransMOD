﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Security.Cryptography;

using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.ValueGenerator.GUID
{
    public class GUIDRequest : IComputeRequest
    {
        public byte[] Result { get; set; }

        public bool IsSuccessful { get; set; }

        public string ErrorMessage { get; set; }

        private int Version { get; set; }

        public string Description
        {
            get
            {
                switch (Version)
                {
                    case 1:
                        return "UUID v1：基于当前时间和网卡 MAC 地址";
                    case 3:
                    case 5:
                        string hashAlgorithm;
                        if (Version == 3)
                        {
                            hashAlgorithm = HashAlgorithmName.MD5.ToString();
                        }
                        else
                        {
                            hashAlgorithm = HashAlgorithmName.SHA1.ToString();
                        }

                        return $"UUID v{Version}：基于给定名称进行 {hashAlgorithm} 散列计算";
                    case 4:
                        return "UUID v4：随机数值";
                    default:
                        return string.Empty;
                }
            }
        }

        private Guid? GuidNamespace { get; set; }

        private string GuidName { get; set; }

        private Guid GuidResult { get; set; }

        private static readonly string NullNamespaceError = $"第一个参数需要是一个 UUID 或者是以下选项之一：{string.Join("、", GUIDGenerator.PredefinedNamespaces.Keys)}";

        public GUIDRequest(int version, string guidNamespace = null, string name = null)
        {
            Version = version;

            if (Version < 1 || Version > 5 || Version == 2)
            {
                throw new ArgumentException("不支持此 GUID 版本，目前仅支持版本 1、3、4、5");
            }

            if (version == 3 || version == 5)
            {
                if (guidNamespace == null)
                {
                    throw new ArgumentNullException(null, NullNamespaceError);
                }

                Guid guid;
                if (GUIDGenerator.PredefinedNamespaces.TryGetValue(guidNamespace.ToLowerInvariant(), out guid))
                {
                    GuidNamespace = guid;
                }
                else if (Guid.TryParse(guidNamespace, out guid))
                {
                    GuidNamespace = guid;
                }
                else
                {
                    throw new ArgumentNullException(null, NullNamespaceError);
                }

                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }
                else
                {
                    GuidName = name;
                }
            }
            else
            {
                GuidNamespace = null;
            }

            ErrorMessage = null;
        }

        public bool Compute()
        {
            IsSuccessful = true;
            try
            {
                switch (Version)
                {
                    case 1:
                        GuidResult = GUIDGenerator.V1();
                        break;
                    case 3:
                        GuidResult = GUIDGenerator.V3(GuidNamespace.Value, GuidName);
                        break;
                    case 4:
                        GuidResult = GUIDGenerator.V4();
                        break;
                    case 5:
                        GuidResult = GUIDGenerator.V5(GuidNamespace.Value, GuidName);
                        break;
                }

                Result = GuidResult.ToByteArray();
            }
            catch (InvalidOperationException e)
            {
                Log.Exception(e.Message, e, GetType());
                ErrorMessage = e.Message;
                IsSuccessful = false;
            }

            return IsSuccessful;
        }

        public string ResultToString()
        {
            if (!IsSuccessful)
            {
                return ErrorMessage;
            }

            return GuidResult.ToString();
        }
    }
}
