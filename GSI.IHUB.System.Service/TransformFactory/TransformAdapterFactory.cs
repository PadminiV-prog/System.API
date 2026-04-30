using GSI.IHUB.System.Service.Contracts;
using GSI.IHUB.System.Service.TransformAdapter;

namespace GSI.IHUB.System.Service.TransformFactory;

public class TransformAdapterFactory : ITransformAdapterFactory
{
    private readonly AppSettings _appSettings;

    public TransformAdapterFactory(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public ITransformAdapter GetAdapter()
    {
        return new GSI.IHUB.System.Service.TransformAdapter.TransformAdapter(_appSettings);
    }
}
