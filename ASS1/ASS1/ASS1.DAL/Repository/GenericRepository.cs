using ASS1.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ASS1.DAL.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Lấy tất cả bản ghi của entity T.
        /// </summary>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách tất cả bản ghi.</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy bản ghi theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi (object).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Bản ghi hoặc null nếu không tìm thấy.</returns>
        public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <summary>
        /// Thêm một bản ghi mới.
        /// </summary>
        /// <param name="entity">Entity cần thêm.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public virtual async Task<bool> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity, cancellationToken);
            var x = await _context.SaveChangesAsync(cancellationToken);
            return x > 0;
        }

        /// <summary>
        /// Thêm nhiều bản ghi mới.
        /// </summary>
        /// <param name="entities">Danh sách entity cần thêm.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null || !entities.Any()) throw new ArgumentNullException(nameof(entities));
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Cập nhật một bản ghi.
        /// </summary>
        /// <param name="entity">Entity cần cập nhật.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _dbSet.Update(entity);
            var x = await _context.SaveChangesAsync(cancellationToken);
            return x > 0;
        }

        /// <summary>
        /// Cập nhật nhiều bản ghi.
        /// </summary>
        /// <param name="entities">Danh sách entity cần cập nhật.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public virtual async Task<bool> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null || !entities.Any()) throw new ArgumentNullException(nameof(entities));
            _dbSet.UpdateRange(entities);
            var x = await _context.SaveChangesAsync(cancellationToken);
            return x > 0;
        }

        /// <summary>
        /// Xóa bản ghi theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi cần xóa.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public virtual async Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                var x = await _context.SaveChangesAsync(cancellationToken);
                return x > 0;
            }
            return false;
        }

        /// <summary>
        /// Xóa nhiều bản ghi theo danh sách ID.
        /// </summary>
        /// <param name="ids">Danh sách ID cần xóa.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) throw new ArgumentNullException(nameof(ids));
            
            // Get the primary key property name
            var entityType = _context.Model.FindEntityType(typeof(T));
            var primaryKey = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();
            if (primaryKey == null) return false;
            
            // Create a parameter expression for the entity
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, primaryKey.Name);
            
            // Create the contains expression
            var containsMethod = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(property.Type);
            
            var idsArray = ids.ToArray();
            var containsExpression = Expression.Call(containsMethod, Expression.Constant(idsArray), property);
            var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
            
            var entities = await _dbSet.Where(lambda).ToListAsync(cancellationToken);
            if (entities.Any())
            {
                _dbSet.RemoveRange(entities);
                var x = await _context.SaveChangesAsync(cancellationToken);
                return x > 0;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra bản ghi có tồn tại theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        public virtual async Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default)
        {
            // Get the primary key property name
            var entityType = _context.Model.FindEntityType(typeof(T));
            var primaryKey = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();
            if (primaryKey == null) return false;
            
            // Create a parameter expression for the entity
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, primaryKey.Name);
            var constant = Expression.Constant(id, property.Type);
            var equalsExpression = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equalsExpression, parameter);
            
            return await _dbSet.AnyAsync(lambda, cancellationToken);
        }

        /// <summary>
        /// Kiểm tra bản ghi có tồn tại theo điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện kiểm tra (ví dụ: x => x.Name == "value").</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách bản ghi thỏa mãn điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện lọc (ví dụ: x => x.Age > 18).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách bản ghi thỏa mãn điều kiện.</returns>
        public virtual async Task<IEnumerable<T>> GetByPredicateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Đếm số bản ghi thỏa mãn điều kiện (nếu có).
        /// </summary>
        /// <param name="predicate">Điều kiện đếm (nếu null, đếm tất cả).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Số lượng bản ghi.</returns>
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            return predicate == null
                ? await _dbSet.CountAsync(cancellationToken)
                : await _dbSet.CountAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách bản ghi theo trang, hỗ trợ lọc, tìm kiếm và sắp xếp.
        /// </summary>
        /// <param name="filters">Danh sách điều kiện lọc (ví dụ: x => x.Status == "Active").</param>
        /// <param name="orderBy">Thuộc tính để sắp xếp (ví dụ: x => x.CreatedDate).</param>
        /// <param name="descending">Sắp xếp giảm dần nếu true, tăng dần nếu false.</param>
        /// <param name="search">Chuỗi tìm kiếm (nếu có).</param>
        /// <param name="searchExpression">Biểu thức tìm kiếm (ví dụ: x => x.Name).</param>
        /// <param name="pageNumber">Số trang (mặc định: 1).</param>
        /// <param name="pageSize">Kích thước trang (mặc định: 10).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách bản ghi và tổng số lượng.</returns>
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetByPageAsync(
            IEnumerable<Expression<Func<T, bool>>>? filters = null,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false,
            string? search = null,
            Expression<Func<T, string>>? searchExpression = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Đảm bảo pageNumber và pageSize hợp lệ
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Max(1, Math.Min(pageSize, 100));

            // Lấy dữ liệu từ database, không theo dõi để tăng tốc
            var query = _dbSet.AsNoTracking();

            // Áp dụng tất cả các bộ lọc
            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = query.Where(filter);
                }
            }

            // Áp dụng tìm kiếm trên biểu thức chỉ định
            if (!string.IsNullOrWhiteSpace(search) && searchExpression != null)
            {
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var propertyToLower = Expression.Call(searchExpression.Body, toLowerMethod!);
                var constant = Expression.Constant(search.ToLower(), typeof(string));
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var containsExpression = Expression.Call(propertyToLower, containsMethod!, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, searchExpression.Parameters[0]);
                query = query.Where(lambda);
            }

            // Áp dụng sắp xếp
            if (orderBy != null)
            {
                query = descending
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }
            else
            {
                // Sắp xếp mặc định theo Primary Key nếu không chỉ định
                var entityType = _context.Model.FindEntityType(typeof(T));
                var primaryKey = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();
                if (primaryKey != null)
                {
                    var parameter = Expression.Parameter(typeof(T));
                    var property = Expression.Property(parameter, primaryKey.Name);
                    var lambda = Expression.Lambda<Func<T, object>>(
                        Expression.Convert(property, typeof(object)),
                        parameter
                    );
                    query = descending
                        ? query.OrderByDescending(lambda)
                        : query.OrderBy(lambda);
                }
            }

            // Chạy tuần tự để tránh lỗi DbContext concurrency
            int totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }

}
