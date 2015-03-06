﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neo4jClient;
using GraphDocs.Models;

namespace GraphDocs.DataServices
{
    public class DocumentsDataService
    {
        private GraphClient client;
        private PathsDataService paths;

        public DocumentsDataService()
        {
            client = DatabaseService.GetConnection();
            paths = new PathsDataService(client);
        }

        public Document Get(string path)
        {
            var documentId = paths.GetIDFromDocumentPath(path);
            var document = client.Cypher
                .WithParams(new { documentId })
                .Match("(d:Document { ID: {documentId} })")
                .Return<Document>("d")
                .Results
                .SingleOrDefault();

            if (document == null)
                return null;

            document.Path = path;
            document.Tags = client.Cypher
                .WithParams(new { documentId })
                .Match("(:Document { ID: {documentId} })<-[:CHILD_OF]-(tag:Tag)")
                .Return<string>("tag.Name")
                .Results
                .ToArray();

            return document;
        }

        public void Delete(string path)
        {
            var id = paths.GetIDFromDocumentPath(path);

            // First delete this document's relationships to parents (probably only one, it's parent folder)
            client.Cypher
                .WithParams(new { id = id })
                .Match("(:Document { ID: {id} })-[rel]->()")
                .Delete("rel")
                .ExecuteWithoutResults();

            // Just delete all relationships to other nodes and this node.  Don't delete any of 
            // the things this is related to, as those may still be in use.
            client.Cypher
                .WithParams(new { id = id })
                .Match("(d:Document { ID: {id} })<-[rels*0..]-()")
                .ForEach("(rel in rels | delete rel)")
                .Delete("d")
                .ExecuteWithoutResults();
        }

        public void Create(Document d)
        {
            if (string.IsNullOrWhiteSpace(d.Name))
                throw new Exception("A document name is required.");

            if (d.ID == null)
                d.ID = Guid.NewGuid().ToString();
            var parentId = paths.GetIDFromFolderPath(d.Path);

            // Attach to root folder
            client.Cypher
                .WithParams(new
                {
                    parentId = parentId,
                    document = new
                    {
                        d.ID,
                        d.Name
                    }
                })
                .Match("(parent:Folder { ID: {parentId} })")
                .Create("parent<-[:CHILD_OF]-(:Document {document})")
                .ExecuteWithoutResults();

            setTags(d.ID, d.Tags);
        }

        public void Save(Document d)
        {
            client.Cypher
                .WithParams(new
                {
                    documentId = d.ID,
                    document = new
                    {
                        d.ID,
                        d.Name
                    }
                })
                .Match("(d:Document { ID: {documentId} })")
                .Set("d = {document}")
                .ExecuteWithoutResults();

            setTags(d.ID, d.Tags);
        }

        private void setTags(string documentId, string[] tags)
        {
            tags = tags ?? new string[] { };
            var existingTags = client.Cypher
                .WithParams(new { documentId })
                .Match("(:Document { ID: {documentId} })<-[:DESCRIBES]-(tag:Tag)")
                .Return<string>("tag.ID")
                .Results
                .ToArray();
            var tagsToDelete = existingTags.Except(tags).ToArray();
            var tagsToAdd = tags.Except(existingTags).ToArray();
            foreach (var tagName in tagsToDelete)
            {
                client.Cypher
                    .WithParams(new { tagName, documentId })
                    .Match("(:Document { ID: {documentId} })<-[:DESCRIBES]-(tag:Tag { Name: {tagName} })")
                    .Delete("tag")
                    .ExecuteWithoutResults();
            }
            foreach (var tagName in tagsToAdd)
            {
                client.Cypher
                    .WithParams(new { tagName, documentId })
                    .Match("(d:Document { ID: {documentId} })")
                    .Merge("(t:Tag { Name: {tagName} })") // Use Merge because we may or may not be creating the tag element, but are definitely creating the vertex.
                    .Create("(d)<-[:DESCRIBES]-(t)")
                    .ExecuteWithoutResults();
            }
        }
    }
}