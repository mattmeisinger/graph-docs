using GraphDocs.Core.Models;

namespace GraphDocs.Core.Interfaces
{
    public interface IDocumentsWorkflowsService
    {
        void InitializeWorkflowsForDocument(string folderId, Document document);
        void SubmitWorkflowReply(Document document, string workflowName, string bookmark, bool response);
    }
}