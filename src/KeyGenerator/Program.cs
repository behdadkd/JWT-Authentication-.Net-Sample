using System.Security.Cryptography;

var key = RSA.Create();
var privateKey = key.ExportRSAPrivateKey();
File.WriteAllBytes("key", privateKey);