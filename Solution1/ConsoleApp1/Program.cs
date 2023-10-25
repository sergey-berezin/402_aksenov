using NugetAnsNetw;
class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("You must enter filePath");
            return;
        }

        string filePath = args[0];
        string text = File.ReadAllText(filePath);
        // read file text

        string modelUrl = "https://storage.yandexcloud.net/dotnet4/bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        string modelPath = "bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        var cts = new CancellationTokenSource();
        // make cancel token, model url and path

        var taskAns = new AnsNetwComp(modelUrl, modelPath, cts.Token);
        // constructor 
        await taskAns.MakeSession();
        // wait when session was set

        var tasks = new List<Task>();
        while (!cts.Token.IsCancellationRequested)
        {
            Console.Write("Enter your question: ");
            string question = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(question))
            {
                cts.Cancel();
                return;
            }
                // no question -> out

            var task = taskAns.Answering(text, question).ContinueWith(task => { Console.WriteLine("Answer on question - '" + question + "' : " + task.Result); }); ;
            // get ans from model

            tasks.Add(task);

        }
        await Task.WhenAll(tasks);
    }
}