using DigitalDocumentLockCommon.Db;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;
using DigitalDocumentLockRepository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalDocumentLockRepository.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Document> _dbSet;

        public DocumentRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Document>();
        }

        public async Task<Document> UploadDocumentAsync(Document doc)
        {
            await _context.Document.AddAsync(doc);
            await _context.SaveChangesAsync();
            return doc;
        }

        public async Task<List<Document>> GetDocumentsByUserAsync(int userId)
        {
            return await _dbSet
                .Where(d => d.UserId == userId && !d.DeleteInd)
                .ToListAsync();
        }

        public async Task<List<Document>> GetAdminDocumentsAsync()
        {
            var result = await _context.Document
                .Include(d => d.User)
                .ToListAsync();

            return result ?? new List<Document>();
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
            return await _dbSet
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DocumentId == documentId && !d.DeleteInd);
        }

        public async Task<List<Document>> GetAllDocumentsWithUserAsync()
        {
            return await _dbSet
                .Include(d => d.User)
                .Where(d => !d.DeleteInd)
                .ToListAsync();
        }

        public async Task<List<Document>> GetUserDocumentsAsync(int userId)
        {
            return await _dbSet
                .Where(d => d.UserId == userId && !d.DeleteInd)
                .ToListAsync();
        }

        public async Task<int> GetActiveDocumentCountAsync()
        {
            return await _dbSet
                .CountAsync(d => !d.DeleteInd);
        }

        public async Task<int> GetTodayUploadedDocumentCountAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet
                .CountAsync(d => d.UploadedAt.Date == today && !d.DeleteInd);
        }

        public async Task SaveDocumentAsync(Document document)
        {
            _context.Document.Update(document);
            await _context.SaveChangesAsync();
        }
    }
}
