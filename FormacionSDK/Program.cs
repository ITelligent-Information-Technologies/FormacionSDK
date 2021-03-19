using Amazon.Batch;
using Amazon.Batch.Model;
using Amazon.CloudWatchEvents;
using Amazon.CloudWatchEvents.Model;
using Amazon.ECS;
using Amazon.ECS.Model;
using System;
using System.Collections.Generic;

namespace FormacionSDK
{
    class Program
    {
        static void Main(string[] args)
        {
            string accessKey = "";
            string secretKey = "";
            Amazon.RegionEndpoint region = Amazon.RegionEndpoint.EUWest1;

            AmazonECSClient ECSClient = new AmazonECSClient(accessKey, secretKey, region);

            RunTaskResponse runTaskResponse = ECSClient.RunTaskAsync(new RunTaskRequest()
            {
                Cluster = "formacionfargate", //nombre del cluster
                Count = 1, //número de tareas
                LaunchType = Amazon.ECS.LaunchType.FARGATE, //tipo de ejecucion
                NetworkConfiguration = new Amazon.ECS.Model.NetworkConfiguration() //configuracion de red
                {
                    AwsvpcConfiguration = new Amazon.ECS.Model.AwsVpcConfiguration() //tipo de red awsvpc, la usada por Fargate
                    {
                        Subnets = new List<string>() { "subnet-a875cfdf"}, //subredes
                        AssignPublicIp = Amazon.ECS.AssignPublicIp.ENABLED //asignar IP púlica
                    }
                },
                Overrides = new TaskOverride() //configuracion de contenedores
                {
                    ContainerOverrides = new List<ContainerOverride>() //lista de contenedores a configurar
                    {
                        new ContainerOverride() //opciones del contenedor
                        {
                            Name = "container", //nombre del contenedor
                            Command = new List<string>(){ "formacion-fargate", "mifichero"} //input del contenedor
                        }
                    }
                },
                TaskDefinition = "formacionfargatetask" //tarea a ejecutar
            }).Result;

            AmazonBatchClient batchClient = new AmazonBatchClient(accessKey, secretKey, region);

            SubmitJobResponse submitJobResponse = batchClient.SubmitJobAsync(new SubmitJobRequest()
            {
                ContainerOverrides = new ContainerOverrides() //opciones de contenedor
                {
                    Command = new List<string>() { "formacion-fargate", "mifichero" } //input del contenedor
                },
                JobDefinition = "formacionbatch", //definicion de trabajo
                JobName = "mi-trabajo", //nombre del trabajo
                JobQueue = "formacionbatch", //cola de trabajo
            }).Result;

            AmazonCloudWatchEventsClient CWEClient = new AmazonCloudWatchEventsClient(accessKey, secretKey, region);

            PutRuleResponse putRuleResponse = CWEClient.PutRuleAsync(new PutRuleRequest()
            {
                Description = "regla para formacion de cloudwatch", //descripcion
                State = RuleState.ENABLED, //activar regla
                Name = "formacioncloudwatch", //nombre de la regla
                ScheduleExpression = "cron(53 * * * ? *)", //activador de la regla
            }).Result;
            PutTargetsResponse putTargetResponse = CWEClient.PutTargetsAsync(new PutTargetsRequest()
            {
                Rule = "formacioncloudwatch", //Nombre de la regla
                Targets = new List<Target>()//lista de destinos
                {
                    new Target()
                    {
                        Id = "destino1", //identificador del destino
                        Arn = "arn:aws:ecs:eu-west-1:837968644020:cluster/formacionfargate", //ARN del cluster
                        EcsParameters = new EcsParameters() //parametros para destinos ECS
                        {
                            LaunchType = Amazon.CloudWatchEvents.LaunchType.FARGATE, //tipo de ejecucion
                            NetworkConfiguration = new Amazon.CloudWatchEvents.Model.NetworkConfiguration() //configuracion de red
                            {
                                AwsvpcConfiguration = new Amazon.CloudWatchEvents.Model.AwsVpcConfiguration() //tipo de red awsvpc, la usada por Fargate
                                {
                                    Subnets = new List<string>() { "subnet-a875cfdf"}, //subredes
                                    AssignPublicIp = Amazon.CloudWatchEvents.AssignPublicIp.ENABLED //asignar IP púlica
                                }
                            },
                            TaskCount = 1, //numero de tareas
                            TaskDefinitionArn = "arn:aws:ecs:eu-west-1:837968644020:task-definition/formacionfargatetask", //ARN de la definicon de tarea
                        },
                        Input = "{\"containerOverrides\":[{\"name\":\"container\",\"command\":[\"formacion-fargate\",\"mifichero\"]}]}", //input
                        RoleArn = "arn:aws:iam::837968644020:role/ecsEventsRole", //ARN del rol de ejecucion
                    }
                },
            }).Result;
        }
    }
}
