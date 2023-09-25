using BudgetProto;
using Backend.Interfaces;
using Grpc.Core;

namespace Backend.Services;

public class MetadataGrpcService : MetadataService.MetadataServiceBase
{
    private readonly IMetadataContext _metadataContext;

    public MetadataGrpcService(IMetadataContext metadataContext)
    {
        _metadataContext = metadataContext;
    }

    public override async Task<AddCategoryResponse> AddCategory(AddCategoryRequest request, ServerCallContext? context)
    {
        if (await _metadataContext.DoesCategoryExist(request.Category))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"The category {request.Category} already exists."));
        }

        await _metadataContext.AddCategory(request.Category);
        return new AddCategoryResponse();
    }

    public override async Task<DeleteCategoryResponse> DeleteCategory(DeleteCategoryRequest request, ServerCallContext? context)
    {
        if (!await _metadataContext.DoesCategoryExist(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist."));
        }

        await _metadataContext.DeleteCategory(request.Category);
        return new DeleteCategoryResponse();
    }

    public override async Task<UpdateCategoryResponse> UpdateCategory(UpdateCategoryRequest request, ServerCallContext? context)
    {
        if (!await _metadataContext.DoesCategoryExist(request.Category))
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"The category {request.Category} does not exist."));
        }

        if (await _metadataContext.DoesCategoryExist(request.UpdateTo))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, $"The category {request.UpdateTo} already exists."));
        }

        await _metadataContext.UpdateCategory(request.Category, request.UpdateTo);
        return new UpdateCategoryResponse();
    }

    public override async Task<GetCategoriesResponse> GetCategories(GetCategoriesRequest request, ServerCallContext? context)
    {
        GetCategoriesResponse response = new();
        response.Categories.Add(await _metadataContext.GetCategories());

        return response;
    }
}