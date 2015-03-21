using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;

namespace WorkflowActivities
{

    public sealed class SubmitOrderName : NativeActivity
    {
        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.CreateBookmark("OrderNameBookmark", new BookmarkCallback(OnBookmarkCallback));
        }

        void OnBookmarkCallback(NativeActivityContext context, Bookmark bookmark, object val)
        {
            Console.WriteLine("Order Name is {0}", (string)val); 
        }        


    }
}
