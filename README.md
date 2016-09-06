# YingYang
C# DotNet multithread/async manager with thread control.

YingYang lets you run async methods directly or in queue on one or multiple controllable threads.


# Example
            YingYang yy = YingYang.getInstance()
                .setPreferedThreadPoolCount(1) //max amount of threads used
                .setMaxThreadCache(1) //max amount of cached/sleaping threads - default value = 1 
                .runWithMainThread(true) //When true yy closes all the threads when the main thread stops - default value = true
                .setThreadSleepTime(0) //Time threads sleeps but kept alive before deconstruction - default = 0
                .addAction(delegate { Console.WriteLine("action"); })         
                .run(delegate
                {
                    Console.WriteLine("yo"); //instant runs action, after that runs actions in queue ( auto run can be disabled  run(code,false))
                });
				//if you dont call run you must call start to start the queue execution
				
YingYang can be used as object and objectfactory.