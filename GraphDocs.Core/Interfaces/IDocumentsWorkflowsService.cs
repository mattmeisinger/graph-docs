using GraphDocs.Core.Models;

namespace GraphDocs.Core.Interfaces
{
    public interface IDocumentsWorkflowsService
    {
        void InitializeWorkflowsForDocument(string folderId, Document document);
        void SubmitWorkflowReply(string workflowInstanceId, string bookmark, bool response);
    }
}