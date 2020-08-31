using Bonsai;
using Bonsai.Audio;
using Bonsai.Dsp;
using Bonsai.Expressions;
using Bonsai.Reactive;
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

        public void Change_val()
        {
            value++;
        }
        public Bonsai_code[] bonsai_Codes;
        public void Launch(string data)
        {


            /*
            var fileName = "Bonsai_files/trial.bonsai"; 
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("Specified workflow file does not exist.");
            }
            
            
            //var audio = workflowBuilder.Workflow.Add(new CombinatorBuilder { Combinator = new ScalarBuffer { Size = new Size(10,10) } });
            var timer = workflowBuilder.Workflow.Add(new CombinatorBuilder { Combinator = new Timer { Period = TimeSpan.FromSeconds(1) } });
            var output = workflowBuilder.Workflow.Add(new WorkflowOutputBuilder());

            workflowBuilder.Workflow.AddEdge(timer, output, new ExpressionBuilderArgument());
            */
            WorkflowBuilder workflowBuilder = new WorkflowBuilder();
            using (var reader = XmlReader.Create(new StringReader(data)))

            {
                workflowBuilder = (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
            }
            var workflow =  workflowBuilder.Workflow.Build();
            var observable = Expression.Lambda<Func<IObservable<Int64>>>(workflow).Compile();
            observable().Subscribe(x => value = (int)x);
            value += 42;
            /*
            

            var workflowCompleted = new ManualResetEvent(false);
            workflowBuilder.Workflow.BuildObservable().Subscribe(
                unit => { },
                ex => { workflowCompleted.Set(); },
                () => workflowCompleted.Set());
            workflowCompleted.WaitOne();
            */
        }
    }
}
