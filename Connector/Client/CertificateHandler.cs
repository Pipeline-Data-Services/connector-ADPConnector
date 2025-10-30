using Connector.Connections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Connector.Client
{
    public class CertificateHandler : HttpClientHandler
    {
        private readonly CustomAuth _customAuth;

        public CertificateHandler(CustomAuth customAuth, string certificatePassword = "")
        {
            _customAuth = customAuth;

            //byte[] pfxBytes = File.ReadAllBytes(@"C:\Users\GuruPrasadReddy\OneDrive - Source One Solutions\Documents\Guru\ADP\myPublicCertificate_AppXchange Spectrum.pfx");
            //string base64Content = Convert.ToBase64String(pfxBytes);
            //Console.WriteLine(base64Content);

            certificatePassword = string.IsNullOrEmpty(certificatePassword) ? _customAuth.CertificatePassword : certificatePassword;

            //string pxfBase64 = "MIILmQIBAzCCC18GCSqGSIb3DQEHAaCCC1AEggtMMIILSDCCBf8GCSqGSIb3DQEHBqCCBfAwggXsAgEAMIIF5QYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYwDgQIQRebyC3vIr8CAggAgIIFuMyPtc3a72R8EmbUILSuXTcxP1fUj07KnTSAe9we2schfES4XrPl7gfr14F2+yiFTBrto/tC6lnPqH4GrlNnI2QZ3QRqxN2kI8dDXcbzz6/eivBfur/rggQM8zkw0YjxQMRqrsXeBsETKhe+t2bb6+e+uQdTyqyjPdlbVR4DNqjT9jvM4kymwT/BslMVlUuA0o8otjlYN1BmzlTBgR8AP/xCioLLKSPFHmwyTVVJgQCemw8lCRfet8WB1V+xzsLWa818+hJkxeqX7+7q7EKdfPFglKE9a7oV93JAoodUvQvi4Sx1y+T8QHE99Z34w/4SVGoOoZEhSzTBSLYCmTzYlDEkw1wln5jlnZ+q6Dtn+gCl7ZIRYHbN72q6KyoWqN+/NwjWm7bY5nPi5QOaiIeKMGN0qH0vruZ8de4C+9c3orYkUHJRiO+cQv+cncQxh793sHHnPkOVSixX4XLpp91GjYOurktLGT180zShbbv4fU3YT/qbbPPJliZJidReELVKg4fyZw66bXNMWOgMV9MqSfB4RQZeQILTi/Zusmiggu4v9aoA15d3WkQyjKZshMIwwkuUtC6gGMbNQeUKD4FkFkSp/UIpneBOmNkxdr/GlbUqj87qhu2I+/P3PjRNFzB5Z9RmVX1jtRYadfsfJh0g+2x83ZE3YwiktabcmYIbZT66K7MbPfrbUobROXMM0g/XY5pd1nQlPjApf9rLxHsPLU9+usBhs4LbycYyIT+Fyen8HWYVN1wNAqzNxhkRjUsh2bSxdZv2bG7XSb4pzxmffab80Ch6/bzP41OEv/o53tzbc9qxN9pNdMq+nu7kFdKAwLyfnrp4MoX2Mm3ZHGCzNXCV8XAi87O/9JSOrMC0ACgOAX4GKy423dFKRxWJdvykKgotSTNM5Fwy3CihMzXmv33EnfLyr34dAXqXUFOY5gG6/+gIv0K1Y2aYTB9nV0tx91+LsOkExfCkYSYP6tYOjDP4jBEwVsicIFc3+AY8p6wWhEsS2Uk1qTmvMWAyatvjNJj+L8B/zE7kxpfh+x/uomgN0EUkNgMuTcrLPtePHjMnTkHl5qtmHr2+YEUxP1chx+Fh3wNrMk0gMrSbweOHxe+g8Qks5bjhaLOlUkLYx0xr4D4K7NF12ZkaheXQyea99qFTvVASqR375o9j8eBT13Oe4OiwV/hunUdrc1iHYvC/sgjLhA2GDCJhLlfaasSjqjozJ2pjlWB+XiyzLRyTpd1BhSV/cGo+KTlAV0woRetRta04l+hFMmKICBWJXcipNiln2zMvFW0UGNyDjvU4iec2+UR6DKli6vrlyLvrDW1t0MUBmSMWYrlZJmfaUlzygSY330gKbzA9s9YP1gE9NIVE5UESb1J2UKJgmXemEISV8jo23klcTlJSfw4s5s5ajczmFMctL23HDtV4zfuXpxFTvCqF+COwipSsX5z13IZD+GTNdMg9JOOg7sJy4hY5/P77+IqQGkn4rUoKXjQLm5jNwFAkmMUf4m/DMqOyLLBuJ68cqJj0hzm9TFttzxRqDaSM0bpqrM/7SjVPOCZwwNCxT81vCn7d9qmhBy8N1XNVc86ssdlFtfrlcC7nM8q7VSjBy5PxWKfC1CIvkCqJIcY5DeHt4VTSox1Y1D9L9Huey3YKYrQ1N0qkiDH2ZT3UcSjyDL/fYYv+32W84FHchChPlLegr8xo7GBjG+jUdKn5dSAA5EiWgrJ7bZEyEvTmCEPyRIRG66UvJRnwQebdcyoAnf9eqQTnPvDwJd34e6xX7hCuFu/esfTBOEM0C35X/TCaRgkHAnk9/8MsROzOrE9ZQeFYdyb3SmVw+TBEtIA5OP2hSvki4vzP0x26qTp4l8Tr2P/Ojhz2NA33xxzVq+yCTob1RchzyXa/ZZ2kIsPQGa/bfk6Db48an7yDmVxicMB5sGXNoPJ4BsFxXeZADcP9ZC8kQD54uTCCBUEGCSqGSIb3DQEHAaCCBTIEggUuMIIFKjCCBSYGCyqGSIb3DQEMCgECoIIE7jCCBOowHAYKKoZIhvcNAQwBAzAOBAja85CDlKAXUQICCAAEggTIsqp8jXFogoDsyZiv7PBz2o48oKo+hzDh2SdC7FRQSznjtCYrQXqe2CGEuv+rAQwhTEqdX192cJN/DClCJHjVxWvfZqpO3yMt50F6pqj5DT9T5xPwPE7656OBJ7mOUlJtqUVDtd0P+wP5/ryyk+U2PfqhHJNIoB6PnKxJr75f25mOOpQpqNQKypC32KYnwpdtTlFAfuBcIrsevsiVCtaLy53UvPApp4/dE+QNln5PA3tvb834tcxGRSR9hkSAK3Yph6b1zFAeQ9OgHQT9QOoOygqrZW3l6xP8CG9L+mr2E7ETaRpxBaQ5YEqTSXx/jCU/m150bTn6f1jqcTaSOZY9vONZaN75n5pazXLMWAvGviIUWkxlD+3NTY7BA5Zu8LdqS35tK2Xoh/EiFAl8qlcBBkWO2ygUw1NJNUMrEtQ2h3/Nul/+Mj82tWAitZwXyTxd2aFWrFwA1Ao86lZd1g4cN7L8jNLeI7XCn17N92qOcFjygCGJp4hhGiO+tTkrC/xJKGyjMo70hi1qhHTFpoUAro8F4/RHf3KiqSzmNITSlvcfVCY5I/GBjzUCwWpnUtfimUiMkA/0wWjkOUmCqYEJAD3Yv5xgMMqJ+6IGyV2xyHrGbjvEqDyXitOwwZQpm37cS1ubH/gMFWk+phi3BbPhyLpZR8vGc8VNMeBdOdVyaCr1XAxiu86/MVGIiVtCGXqPli9z3oNKhExRcajQLXRG66QC3KL466e+AJe+pl+OdhYUOd/zF3McRLfTDOpJBp1B+YBkcl5cRQT4YcvEpC8/8znBJZfkZhPhciciBHk9ioZhwAd3+f+YZpyHL0UxGFfS8GJ36F9+IQP5pqq/mYzOGBh5Mgs1l5h/NQP7Vshqx85Rih6bzWIc58Q2eV0cMQW/Nmj1vJWHWcwjvE34RNSRKnV/ZU6i1etr/p+iqs8D2EXp6HFWbkHadqNVOd5nWoOSljSvnZiLDetFqn9tbWFzkt4PKy66v8yCElAIVHGalkd5mjfwfIfarQHNqWCNax+HC/SUlaCLnu92nTUI3H2F9zlhfNjQ5AU1NJEwkv96GfCkE/BvahPgjfminwnatmbiKFK/7UIyATNocHmPIthLaLNxW1kLFY8rIzsPM8rRjByI6DYcmMwsp4fos1ThvgQsO3EYK7HT1ufANPhjKrKbAAMINh07W3tDiqB9/GXm9ZsslgQQWhKxWLxO7+ZE+N9oRYfeEkAFLMGERAZCugrQlSQqXr6vAFnFZ8xshnaF5tM+7li7xUSBzm8U62DWZW1dpkzBfqZ1r7kDD4iKfGCIgEtAm9fkxbyVLdbyh5+Kx06mblyNING+2tUboixE4dgZffYxzql5nALLAg5b7Fnm7+/ODQRVd9JbAIriNd7yDklO4DZnzVVocmsKaIWt56bjkk5M8vhdKBFkWCO5jVCeooynUwD7NrY1V7hwbXqUINvtUTu7lAMbo3iHZKv/7YWT51OjV4CwUAYYkep8kwjfWzBiJb+d6YS0HMzNuasirVWdUDkcISN2D/gIMa6hMS90lcvsG5q2I1g+9uutHP+O8kd2yDRLRWo0distNdwOdBNxV4hvZaKwVaWW42jDV1CapqHqkr6TsSxMaMFvPBUlqTx3wXiW3cWfMSUwIwYJKoZIhvcNAQkVMRYEFMyTEiJPW4YVN4lHKvpEjiCQDJ2MMDEwITAJBgUrDgMCGgUABBSX+nlshSYv8oxh2CK3a/b5xoz78gQIDULYymPPuZsCAggA";
            string pxfBase64 = _customAuth.ClientCertBase64 ;

            // Convert Base64 string to byte array
            byte[] certBytes = Convert.FromBase64String(pxfBase64);

            // Create X509Certificate2 from byte array with password
            var certificate = new X509Certificate2(certBytes, certificatePassword);
            
            ClientCertificateOptions = ClientCertificateOption.Manual;
            ClientCertificates.Add(certificate);
        }
    }
}
