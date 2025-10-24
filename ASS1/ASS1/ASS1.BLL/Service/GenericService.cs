using ASS1.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ASS1.BLL.Service
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        private readonly IGenericRepository<T> _repository;

        public GenericService(IGenericRepository<T> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Lấy tất cả bản ghi của entity T.
        /// </summary>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách tất cả bản ghi.</returns>
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _repository.GetAllAsync(cancellationToken);
        }

        /// <summary>
        /// Lấy bản ghi theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi (object).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Bản ghi hoặc null nếu không tìm thấy.</returns>
        public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _repository.GetByIdAsync(id, cancellationToken);
        }

        /// <summary>
        /// Thêm bản ghi mới.
        /// </summary>
        /// <param name="entity">Entity cần thêm.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Entity vừa thêm.</returns>
        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _repository.AddAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Thêm nhiều bản ghi mới.
        /// </summary>
        /// <param name="entities">Danh sách entity cần thêm.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null || !entities.Any()) throw new ArgumentNullException(nameof(entities));
            await _repository.AddRangeAsync(entities, cancellationToken);
        }

        /// <summary>
        /// Cập nhật bản ghi.
        /// </summary>
        /// <param name="id">ID của bản ghi cần cập nhật.</param>
        /// <param name="entity">Entity chứa dữ liệu cập nhật.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Entity vừa cập nhật.</returns>
        public async Task<T> UpdateAsync(object id, T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
            if (existingEntity == null) throw new KeyNotFoundException($"Entity with ID {id} not found.");
            // Giả định ánh xạ DTO sang entity đã được xử lý trước khi gọi
            await _repository.UpdateAsync(entity, cancellationToken);
            return entity;
        }

        /// <summary>
        /// Xóa bản ghi theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi cần xóa.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public async Task DeleteAsync(object id, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(id, cancellationToken);
        }

        /// <summary>
        /// Xóa nhiều bản ghi theo danh sách ID.
        /// </summary>
        /// <param name="ids">Danh sách ID cần xóa.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        public async Task DeleteRangeAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null || !ids.Any()) throw new ArgumentNullException(nameof(ids));
            await _repository.DeleteRangeAsync(ids, cancellationToken);
        }

        /// <summary>
        /// Kiểm tra bản ghi có tồn tại theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        public async Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default)
        {
            return await _repository.ExistsAsync(id, cancellationToken);
        }

        /// <summary>
        /// Kiểm tra bản ghi có tồn tại theo điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện kiểm tra (ví dụ: x => x.Name == "value").</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _repository.ExistsAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Lấy danh sách bản ghi thỏa mãn điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện lọc (ví dụ: x => x.Age > 18).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách bản ghi thỏa mãn điều kiện.</returns>
        public async Task<IEnumerable<T>> GetByPredicateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return await _repository.GetByPredicateAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Đếm số bản ghi thỏa mãn điều kiện (nếu có).
        /// </summary>
        /// <param name="predicate">Điều kiện đếm (nếu null, đếm tất cả).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Số lượng bản ghi.</returns>
        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
        {
            return await _repository.CountAsync(predicate, cancellationToken);
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
        public async Task<(IEnumerable<T> Items, int TotalCount)> GetByPageAsync(
            IEnumerable<Expression<Func<T, bool>>>? filters = null,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false,
            string? search = null,
            Expression<Func<T, string>>? searchExpression = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return await _repository.GetByPageAsync(filters, orderBy, descending, search, searchExpression, pageNumber, pageSize, cancellationToken);
        }
    }

}
