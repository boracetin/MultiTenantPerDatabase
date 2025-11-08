namespace MultitenantPerDb.Core.Application;

/// <summary>
/// Base service class - marker for UnitOfWork access authorization
/// All application services must inherit from this class to access UnitOfWork
/// Enforced by MTDB003 analyzer at compile-time
/// 
/// WHY NON-GENERIC:
/// ✅ Simple - No generic complexity
/// ✅ Flexible - Services inject their own UnitOfWork(s) as needed
/// ✅ One service can use multiple UnitOfWorks (MainDb + ApplicationDb)
/// 
/// USAGE:
/// public class ProductService : BaseService, IProductService
/// {
///     private readonly IUnitOfWork<ApplicationDbContext> _unitOfWork;
///     
///     public ProductService(IUnitOfWork<ApplicationDbContext> unitOfWork)
///     {
///         _unitOfWork = unitOfWork;
///     }
/// }
/// 
/// MULTIPLE CONTEXTS:
/// public class CrossTenantService : BaseService
/// {
///     private readonly IUnitOfWork<MainDbContext> _mainUnitOfWork;
///     private readonly IUnitOfWork<ApplicationDbContext> _appUnitOfWork;
///     
///     public CrossTenantService(
///         IUnitOfWork<MainDbContext> mainUnitOfWork,
///         IUnitOfWork<ApplicationDbContext> appUnitOfWork)
///     {
///         _mainUnitOfWork = mainUnitOfWork;
///         _appUnitOfWork = appUnitOfWork;
///     }
/// }
/// </summary>
public abstract class BaseService : Domain.ICanAccessUnitOfWork
{
    protected BaseService()
    {
    }
}
