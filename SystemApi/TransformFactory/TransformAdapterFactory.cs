using SystemApi.Contracts;
using SystemApi.TransformAdapter;

namespace SystemApi.TransformFactory;

public class TransformAdapterFactory : ITransformAdapterFactory
{
    private readonly AppSettings _appSettings;

    public TransformAdapterFactory(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public ITransformAdapter GetAdapter()
    {
        return new DCTransformAdapter(_appSettings);
    }
}
