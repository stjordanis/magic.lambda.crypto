﻿/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.crypto.slots.hash
{
    /// <summary>
    /// [crypto.hash] slot to create a cryptographically secure hash of a piece of string.
    /// </summary>
    [Slot(Name = "crypto.hash")]
    public class Hash : ISlot
    {
        /// <summary>
        /// Implementation of slot.
        /// </summary>
        /// <param name="signaler">Signaler invoking slot.</param>
        /// <param name="input">Arguments to slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var contentRaw = input.GetEx<object>();
            var data = contentRaw is string strContent ? Encoding.UTF8.GetBytes(strContent) : contentRaw as byte[];
            var algorithm = input.Children.FirstOrDefault(x => x.Name == "algorithm")?.GetEx<string>() ?? "SHA256";
            var format = input.Children.FirstOrDefault(x => x.Name == "format")?.GetEx<string>() ?? "text";
            switch (algorithm)
            {
                case "SHA256":
                    using (var algo = SHA256Managed.Create())
                    {
                        input.Value = GenerateHash(algo, data, format);
                    }
                    break;
                case "SHA384":
                    using (var algo = SHA384Managed.Create())
                    {
                        input.Value = GenerateHash(algo, data, format);
                    }
                    break;
                case "SHA512":
                    using (var algo = SHA512Managed.Create())
                    {
                        input.Value = GenerateHash(algo, data, format);
                    }
                    break;
                default:
                    throw new ArgumentException($"'{algorithm}' is an unknown hashing algorithm.");
            }

            // House cleaning.
            input.Clear();
        }

        #region [ -- Private helper methods -- ]

        object GenerateHash(HashAlgorithm algo, byte[] data, string format)
        {
            var bytes = algo.ComputeHash(data);
            switch (format)
            {
                case "text":
                    return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
                case "raw":
                    return bytes;
                case "fingerprint":
                    var result = new StringBuilder();
                    var idxNo = 0;
                    foreach (var idx in bytes)
                    {
                        result.Append(BitConverter.ToString(new byte[] { idx }));
                        if (++idxNo % 2 == 0)
                            result.Append("-");
                    }
                    return result.ToString().TrimEnd('-').ToLowerInvariant();
                default:
                    throw new ArgumentException($"I don't understand {format} as format for my hash");
            }
        }

        #endregion
    }
}
