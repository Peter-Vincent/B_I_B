using Bonsai;
using Bonsai.Audio;
using Bonsai.Dsp;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Bonsai.Windows.Input;
using Bonsai.IO;
using OpenCV.Net;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Xml;
using System.Reactive.Subjects;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;

namespace Bonsai_test2
{
    public class Bonsai_code
    {
        public string Summary { get; set; }
    }

    
    public class ExternalEventSource<TSource> : Source<TSource>
    {
        public ISubject<TSource> Subject { get; } = new Subject<TSource>();
        public override IObservable<TSource> Generate()
        {
            return Subject;
        }
    }
    public class Bonsai_func
    {
        public int value = 0;
        public int value2 = 0;

        public void Change_val()
        {
            value++;
        }
        //public Bonsai_code[] bonsai_Codes;

        public void Launch_screen(string data)
        {
            WorkflowBuilder workflowBuilder = new WorkflowBuilder();
            using (var reader = XmlReader.Create(new StringReader(data)))

            {
                workflowBuilder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }
            workflowBuilder.Workflow.Build();
        }

        /// <summary>
        /// Code to get dynamic updating of the mous location and clicks
        /// </summary>
        public ExternalEventSource<MouseEventArgs> mouse_move = new ExternalEventSource<MouseEventArgs>();

        public IObservable<MouseEventArgs> Launch_mouse()
        {
            WorkflowBuilder workflowBuilderMouse = new WorkflowBuilder();
            var mouse = workflowBuilderMouse.Workflow.Add(new CombinatorBuilder { Combinator = mouse_move });
            var output = workflowBuilderMouse.Workflow.Add(new WorkflowOutputBuilder());

            workflowBuilderMouse.Workflow.AddEdge(mouse, output, new ExpressionBuilderArgument());
            var workflow = workflowBuilderMouse.Workflow.Build();
            var observable = Expression.Lambda<Func<IObservable<MouseEventArgs>>>(workflow).Compile();
            return observable();
        }


        /// <summary>
        /// Manual bonsai code to run the keyboard
        /// </summary>
        public ExternalEventSource<KeyboardEventArgs> event_source = new ExternalEventSource<KeyboardEventArgs>();
        public void Launch_key()
        {
             
            WorkflowBuilder workflowBuilderKey = new WorkflowBuilder();
            var keypress = workflowBuilderKey.Workflow.Add(new CombinatorBuilder { Combinator = event_source });
            var int_node = workflowBuilderKey.Workflow.Add(new CombinatorBuilder { Combinator = new Bonsai.Expressions.Int64Property { Value = 1} });
            var accumulate = workflowBuilderKey.Workflow.Add(new CombinatorBuilder { Combinator = new Bonsai.Reactive.Accumulate { } });
            var output   = workflowBuilderKey.Workflow.Add(new WorkflowOutputBuilder());

            workflowBuilderKey.Workflow.AddEdge(keypress, int_node, new ExpressionBuilderArgument());
            workflowBuilderKey.Workflow.AddEdge(int_node, accumulate, new ExpressionBuilderArgument());
            workflowBuilderKey.Workflow.AddEdge(accumulate, output, new ExpressionBuilderArgument());
            var workflow2 = workflowBuilderKey.Workflow.Build();
            var observable2 = Expression.Lambda<Func<IObservable<Int64>>>(workflow2).Compile();
            observable2().Subscribe(x => value2 = (int)x);
            value2 += 24;
        }
        public IObservable<Int64> Launch(string data)
        {
            
            WorkflowBuilder workflowBuilder = new WorkflowBuilder();
            using (var reader = XmlReader.Create(new StringReader(data)))

            {
                workflowBuilder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }
            var workflow =  workflowBuilder.Workflow.Build();
            var observable = Expression.Lambda<Func<IObservable<Int64>>>(workflow).Compile();
            return observable();
            //observable().Subscribe(x =>  value = (int)x);
            
        }
    }
}
