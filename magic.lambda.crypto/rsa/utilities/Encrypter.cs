﻿/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto;
using magic.node;
using magic.node.extensions;
using magic.lambda.crypto.utilities;

namespace magic.lambda.crypto.rsa.utilities
{
    /*
     * Utility class to provide common functions for other classes and methods.
     */
    internal static class Encrypter
    {
        /*
         * Encrypts a message using the specified engine, and returns result to
         * caller, according to caller's specifications.
         */
        internal static void EncryptMessage(Node input, IAsymmetricBlockCipher engine)
        {
            // Retrieving message and other arguments.
            var rawMessage = input.GetEx<object>();
            var message = rawMessage is string strMsg ? Encoding.UTF8.GetBytes(strMsg) : rawMessage as byte[];

            var raw = input.Children.FirstOrDefault(x => x.Name == "raw")?.GetEx<bool>() ?? false;
            var key = Utilities.GetPublicKey(input);
            var cipher = Utilities.EncryptMessage(engine, message, key);
            input.Value = raw ? cipher : (object)Convert.ToBase64String(cipher);
            input.Clear();
        }
    }
}
