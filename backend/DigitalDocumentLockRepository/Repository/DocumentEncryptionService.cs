using DigitalDocumentLockCommon.Models; 
using iText.Kernel.Pdf;
using Microsoft.Extensions.Options;
using System;
using System.Text;

namespace DigitalDocumentLockRepository.Repository
{
    public class DocumentEncryptionService
    {
        private readonly string _adminPassword;

        public DocumentEncryptionService(IOptions<DocumentEncryptionSettings> options)
        {
            _adminPassword = options.Value.AdminPassword;
        }


        public void EncryptPdf(string inputFilePath, string outputFilePath, string userPassword)
        {
            var writerProperties = new WriterProperties()
                .SetStandardEncryption(
                    Encoding.UTF8.GetBytes(userPassword),        // User password
                    Encoding.UTF8.GetBytes(_adminPassword),      // Static admin password
                    EncryptionConstants.ALLOW_PRINTING,          // Permissions
                    EncryptionConstants.ENCRYPTION_AES_128 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);

            using var pdfReader = new PdfReader(inputFilePath);
            using var pdfWriter = new PdfWriter(outputFilePath, writerProperties);
            using var pdfDoc = new PdfDocument(pdfReader, pdfWriter);
        }

        public void EncryptWord(string inputFilePath, string outputFilePath, string password)
        {
            throw new NotImplementedException("Word encryption is not supported yet.");
        }

        public string DecryptPdfTemporarily(string encryptedPdfPath, string password)
        {
            var decryptedPath = Path.Combine(
                Path.GetDirectoryName(encryptedPdfPath)!,
                Path.GetFileNameWithoutExtension(encryptedPdfPath) + "_preview.pdf"
            );

            var readerProperties = new ReaderProperties()
                .SetPassword(Encoding.UTF8.GetBytes(password));

            using var reader = new PdfReader(encryptedPdfPath, readerProperties);
            using var writer = new PdfWriter(decryptedPath);
            using var pdf = new PdfDocument(reader, writer);

            return decryptedPath;
        }

    }
}
