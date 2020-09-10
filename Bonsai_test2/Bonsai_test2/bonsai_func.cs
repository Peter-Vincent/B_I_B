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

namespace Bonsai_test2
{
    public class Bonsai_code
    {
        public string Summary { get; set; }
    }
    public class Bonsai_func
    {
        public int value = 0;
        public int value2 = 0;

        public void Change_val()
        {
            value++;
        }
        public Bonsai_code[] bonsai_Codes;
        
        public void Launch_key()
        {
            WorkflowBuilder workflowBuilder2 = new WorkflowBuilder();
            var keypress = workflowBuilder2.Workflow.Add(new CombinatorBuilder { Combinator = new KeyDown { } });
            var int_node = workflowBuilder2.Workflow.Add(new CombinatorBuilder { Combinator = new Bonsai.Expressions.Int64Property { Value = 1} });
            var accumulate = workflowBuilder2.Workflow.Add(new CombinatorBuilder { Combinator = new Bonsai.Reactive.Accumulate { } });
            var output   = workflowBuilder2.Workflow.Add(new WorkflowOutputBuilder());

            workflowBuilder2.Workflow.AddEdge(keypress, int_node, new ExpressionBuilderArgument());
            workflowBuilder2.Workflow.AddEdge(int_node, accumulate, new ExpressionBuilderArgument());
            workflowBuilder2.Workflow.AddEdge(accumulate, output, new ExpressionBuilderArgument());
            var workflow2 = workflowBuilder2.Workflow.Build();
            var observable2 = Expression.Lambda<Func<IObservable<Int64>>>(workflow2).Compile();
            observable2().Subscribe(x => value2 = (int)x);
            value2 += 24;
        }
        public void Launch(string data)
        {

            WorkflowBuilder workflowBuilder = new WorkflowBuilder();
            using (var reader = XmlReader.Create(new StringReader(data)))

            {
                workflowBuilder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }
            var workflow =  workflowBuilder.Workflow.Build();
            var observable = Expression.Lambda<Func<IObservable<Int64>>>(workflow).Compile();
            observable().Subscribe(x => value = (int)x);
            value += 42;
        }
    }
}
