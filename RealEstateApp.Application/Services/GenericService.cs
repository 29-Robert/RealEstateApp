using AutoMapper;
using RealEstateApp.Application.Interfaces.Repositories;
using RealEstateApp.Application.Interfaces.Services;

namespace RealEstateApp.Application.Services
{
    public class GenericService<SaveViewModel, ViewModel, Entity>(IGenericRepository<Entity> repository, IMapper mapper) : IGenericService<SaveViewModel, ViewModel, Entity>
        where SaveViewModel : class
        where ViewModel : class
        where Entity : class
    {
        private readonly IGenericRepository<Entity> _repository = repository;
        private readonly IMapper _mapper = mapper;

        public virtual async Task<List<ViewModel>> GetAllViewModel()
        {
            var entityList = await _repository.GetAllAsync();
            return _mapper.Map<List<ViewModel>>(entityList);
        }

        public virtual async Task<SaveViewModel?> GetByIdSaveViewModel(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<SaveViewModel>(entity);
        }

        public virtual async Task<SaveViewModel> Add(SaveViewModel vm)
        {
            var entity = _mapper.Map<Entity>(vm);
            entity = await _repository.AddAsync(entity);
            return _mapper.Map<SaveViewModel>(entity);
        }

        public virtual async Task Update(SaveViewModel vm)
        {
            var entity = _mapper.Map<Entity>(vm);
            await _repository.UpdateAsync(entity);
        }

        public virtual async Task Delete(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity != null)
            {
                await _repository.DeleteAsync(entity);
            }
        }
    }
}