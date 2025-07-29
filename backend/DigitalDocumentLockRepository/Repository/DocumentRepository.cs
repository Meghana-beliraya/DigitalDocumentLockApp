using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using DigitalDocumentLockCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalDocumentLockRepository.Repository;

namespace DigitalDocumentLockRepository.Repositories
{
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        private readonly AppDbContext _context;

        public DocumentRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Document>> GetDocumentsByUserAsync(int userId)
        {
            return await _context.Document
                .Where(d => d.UserId == userId && !d.DeleteInd)
                .ToListAsync();
        }

        public async Task<List<Document>> GetAdminDocumentsAsync()
        {
            return await _context.Document.Include(d => d.User).ToListAsync();
        }

        public async Task<List<UserSummaryDto>> GetAllUserSummariesAsync()
        {
            return await _context.Users
                .Where(u => !u.IsAdmin)
                .Select(u => new UserSummaryDto
                {
                    UserId = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    TotalDocumentsUploaded = _context.Document.Count(d => d.UserId == u.Id && !d.DeleteInd)
                })
                .ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(int documentId)
        {
            return await _context.Document
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.DeleteInd);
        }

        public async Task<List<Document>> GetAllDocumentsWithUserAsync()
        {
            return await _context.Document
                .Include(d => d.User)
                .Where(d => !d.DeleteInd)
                .ToListAsync();
        }

        public async Task<List<Document>> GetUserDocumentsAsync(int userId)
        {
            return await _context.Document
                .Where(d => d.UserId == userId && !d.DeleteInd)
                .ToListAsync();
        }

        public async Task<int> GetActiveDocumentCountAsync()
        {
            return await _context.Document.CountAsync(d => !d.DeleteInd);
        }

        public async Task<int> GetTodayUploadedDocumentCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Document.CountAsync(d => d.UploadedAt.Date == today && !d.DeleteInd);
        }

        public async Task SaveDocumentAsync(Document document)
        {
            _context.Document.Update(document); // no SaveChangesAsync here; handled by UnitOfWork
        }

        public async Task UploadDocumentAsync(Document doc)
        {
            await _context.Document.AddAsync(doc); // no SaveChangesAsync here; handled by UnitOfWork
        }

        public async Task<Document?> GetDocumentByIdAndUserAsync(int documentId, int userId)
        {
            return await _context.Document
                .FirstOrDefaultAsync(d => d.DocumentId == documentId && d.UserId == userId && !d.DeleteInd);
        }

    }
}
