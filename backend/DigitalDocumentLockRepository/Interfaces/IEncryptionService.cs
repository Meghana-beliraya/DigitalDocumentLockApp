using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Interfaces
{
    public interface IEncryptionService
    {
        void EncryptPdf(string inputPath, string outputPath, string password);
        string DecryptPdfTemporarily(string encryptedPdfPath, string password);

    }
}
