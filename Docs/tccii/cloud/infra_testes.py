from diagrams import Diagram, Cluster, Edge
from diagrams.aws.compute import EC2, AutoScaling
from diagrams.aws.network import ELB
from diagrams.onprem.monitoring import Grafana, Prometheus
from diagrams.onprem.database import InfluxDB

with Diagram("Infra Testes", show=False):
    with Cluster("AWS"):
      with Cluster("Simulando sistema do cliente"):
        # Client k6
        k6_ec2 = EC2("k6")

        with Cluster("cep-promise"):
          # Cliente que usa o cep-promise
          lb_cp = ELB("lb")
          with Cluster("Javascript app \ncom cep-promise"):
              cp_ec2_group = AutoScaling("server")

        with Cluster("broker"):
          # Cliente que usa o Broker
          lb_broker = ELB("lb")
          with Cluster("Javascript app"):
              broker_ec2_group = [EC2("server"),EC2("server")]

          # Broker
          with Cluster("Broker"):
            influxdb = InfluxDB("InfluxDB")
            broker = EC2("Broker")
        
        # Monitoramento
        metrics_ec2 = Prometheus("Prometheus")
        metrics_ec2 >> cp_ec2_group
        metrics_ec2 >> broker_ec2_group
        metrics_ec2 << Edge(color="firebrick", style="dashed") << Grafana("monitoring")

      # Provedores
      with Cluster("Simulando a internet"):
          providers_group = [EC2("provedor 1"),
                             EC2("provedor 2"),
                             EC2("provedor 3")]

      k6_ec2 >> lb_cp
      k6_ec2 >> lb_broker

      lb_cp >> cp_ec2_group
      lb_broker >> broker_ec2_group

      cp_ec2_group >> providers_group
      broker_ec2_group >> broker >> Edge(labelangle="0", labeldistance="14", constraint="False") >> influxdb
      broker >> providers_group