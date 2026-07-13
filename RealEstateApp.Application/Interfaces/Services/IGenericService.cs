namespace RealEstateApp.Application.Interfaces.Services
{
    public interface IGenericService<SaveViewModel, ViewModel, Entity>
        where SaveViewModel : class
        where ViewModel : class
        where Entity : class
    {
        Task<List<ViewModel>> GetAllViewModel();
        Task<SaveViewModel?> GetByIdSaveViewModel(int id);
        Task<SaveViewModel> Add(SaveViewModel vm);
        Task Update(SaveViewModel vm);
        Task Delete(int id);
    }
}