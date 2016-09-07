# YingYang
C# DotNet multithread/async manager with thread control.

YingYang lets you run async methods directly or in queue on one or multiple controllable threads.


# Example
```c#
	YingYang.getInstance()
		.setPreferedThreadPoolCount(1) //max amount of threads used
		.setMaxThreadCache(1) //max amount of cached/sleaping threads - default value = 1 
		.runWithMainThread(true) //When true yy closes all the threads when the main thread stops - default value = true
		.setThreadSleepTime(0) //Time threads sleeps but kept alive before deconstruction - default = 0
		.addAction(delegate { Console.WriteLine("action"); })         
		.run(delegate
		{
			Console.WriteLine("action"); //instant runs action, after that runs actions in queue ( auto run can be disabled  run(code,false))
		});
		//if you dont call run you must call start to start the queue execution
```			

YingYang can also be stored in a object to reach the active threads


## License
	Copyright 2016 DreamInCode B.V.

	Licensed under the Apache License, Version 2.0 (the "License");
	you may not use this file except in compliance with the License.
	You may obtain a copy of the License at

	   http://www.apache.org/licenses/LICENSE-2.0

	Unless required by applicable law or agreed to in writing, software
	distributed under the License is distributed on an "AS IS" BASIS,
	WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	See the License for the specific language governing permissions and
	limitations under the License.