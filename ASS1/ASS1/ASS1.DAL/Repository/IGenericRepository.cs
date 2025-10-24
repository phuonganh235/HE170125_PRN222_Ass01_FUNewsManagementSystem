using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ASS1.DAL.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Lấy tất cả bản ghi của entity T.
        /// </summary>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách tất cả bản ghi.</returns>
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy bản ghi theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi (object).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Bản ghi hoặc null nếu không tìm thấy.</returns>
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Thêm một bản ghi mới.
        /// </summary>
        /// <param name="entity">Entity cần thêm.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        Task<bool> AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Thêm nhiều bản ghi mới.
        /// </summary>
        /// <param name="entities">Danh sách entity cần thêm.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật một bản ghi.
        /// </summary>
        /// <param name="entity">Entity cần cập nhật.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật nhiều bản ghi.
        /// </summary>
        /// <param name="entities">Danh sách entity cần cập nhật.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        Task<bool> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa bản ghi theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi cần xóa.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        Task<bool> DeleteAsync(object id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa nhiều bản ghi theo danh sách ID.
        /// </summary>
        /// <param name="ids">Danh sách ID cần xóa.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        Task<bool> DeleteRangeAsync(IEnumerable<object> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra bản ghi có tồn tại theo ID.
        /// </summary>
        /// <param name="id">ID của bản ghi.</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra bản ghi có tồn tại theo điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện kiểm tra (ví dụ: x => x.Name == "value").</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>True nếu tồn tại, False nếu không.</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách bản ghi thỏa mãn điều kiện.
        /// </summary>
        /// <param name="predicate">Điều kiện lọc (ví dụ: x => x.Age > 18).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Danh sách bản ghi thỏa mãn điều kiện.</returns>
        Task<IEnumerable<T>> GetByPredicateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm số bản ghi thỏa mãn điều kiện (nếu có).
        /// </summary>
        /// <param name="predicate">Điều kiện đếm (nếu null, đếm tất cả).</param>
        /// <param name="cancellationToken">Token để hủy thao tác.</param>
        /// <returns>Số lượng bản ghi.</returns>
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);

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
        Task<(IEnumerable<T> Items, int TotalCount)> GetByPageAsync(
            IEnumerable<Expression<Func<T, bool>>>? filters = null,
            Expression<Func<T, object>>? orderBy = null,
            bool descending = false,
            string? search = null,
            Expression<Func<T, string>>? searchExpression = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }

}
