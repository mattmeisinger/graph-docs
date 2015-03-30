
using Neo4jClient;

namespace GraphDocs.Core.Interfaces
{
    public interface IConnectionFactory
    {
        IGraphClient GetConnection();
        void InitAndEraseAll();
        void Init();
        void EraseAll();
    }
}
