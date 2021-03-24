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
            string strIdCuenta = "";

            //Ejecucion de tarea en Fargate
            AmazonECSClient ECSClient = new AmazonECSClient(accessKey, secretKey, region);

            RunTaskResponse runTaskResponse = ECSClient.RunTaskAsync(new RunTaskRequest()
            {
                Cluster = "practica1", //nombre del cluster
                Count = 1, //número de tareas
                LaunchType = Amazon.ECS.LaunchType.FARGATE, //tipo de ejecucion
                NetworkConfiguration = new Amazon.ECS.Model.NetworkConfiguration() //configuracion de red
                {
                    AwsvpcConfiguration = new Amazon.ECS.Model.AwsVpcConfiguration() //tipo de red awsvpc, la usada por Fargate
                    {
                        Subnets = new List<string>() { "subnet-831004e5" }, //subredes
                        AssignPublicIp = Amazon.ECS.AssignPublicIp.ENABLED //asignar IP púlica
                    }
                },
                Overrides = new TaskOverride() //configuracion de contenedores
                {
                    ContainerOverrides = new List<ContainerOverride>() //lista de contenedores a configurar
                    {
                        new ContainerOverride() //opciones del contenedor
                        {
                            Name = "FormacionFargateTask", //nombre del contenedor
                            Command = new List<string>(){ "formacion-batch-sieca", "mifichero"} //input del contenedor
                        }
                    }
                },
                TaskDefinition = "FormacionFargateTask" //tarea a ejecutar
            }).Result;


            //Ejecucion de trabajo en Batch
            AmazonBatchClient batchClient = new AmazonBatchClient(accessKey, secretKey, region);

            SubmitJobResponse submitJobResponse = batchClient.SubmitJobAsync(new SubmitJobRequest()
            {
                ContainerOverrides = new ContainerOverrides() //opciones de contenedor
                {
                    Command = new List<string>() { "formacion-batch-sieca", "mifichero" } //input del contenedor
                },
                JobDefinition = "formacionBatch", //definicion de trabajo
                JobName = "mi-trabajo", //nombre del trabajo
                JobQueue = "formacionBatch", //cola de trabajo
            }).Result;

        }
    }
}
