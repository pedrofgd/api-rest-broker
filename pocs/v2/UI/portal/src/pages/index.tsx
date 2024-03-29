import Head from 'next/head'
import { Inter } from 'next/font/google'
import styles from '@/styles/Home.module.css'
import { LogLevel, HubConnectionBuilder } from '@microsoft/signalr';
import { ReactElement, useState } from 'react';

const inter = Inter({ subsets: ['latin'] })

const brokerHost = process.env.BROKER_IP ?? "localhost:5070";
console.log("BROKER_IP =", brokerHost)

const connection = new HubConnectionBuilder()
  .withUrl(`http://${brokerHost}/ranqueamento`)
  .configureLogging(LogLevel.Information)
  .build();

async function startConnection() {
  try {
    await connection.start();
    console.log("SignalR Connected.")
  } catch (err) {
    console.log("Error starting connection with SignalR:", err);
    setTimeout(startConnection, 5000);
  };

  connection.onclose(async () => {
    await startConnection();
  });
};

// Start the connection with websocket server
startConnection();

export default function Home() {
  const [providers, setProviders] = useState<String[]>([]);
  const [active, setActive] = useState<String>();
  const [result, setResult] = useState<ReactElement[]>([])

  connection.on('ReceiveMessage', (providers: String[], activeProvider: String) => {
    setProviders(providers)
    setActive(activeProvider)
    let temp: ReactElement[] = []
    providers.forEach((provider, position) => {
      temp.push(
        <section className={`${styles.section} ${inter.className}`}>
          <h2 className={`${styles.h2} ${inter.className}`}>
            Posição: {position + 1}
          </h2>
          <h1 className={`${styles.h1} ${inter.className}`}>
            Provedor: {provider}
          </h1>
        </section>)
    });

    setResult(temp)
    console.log(`Received message from ${providers}: ${activeProvider}`);
  });


  return (
    <>
      <Head>
        <title>Broker Portal</title>
        <meta name="description" content="Generated by create next app" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <main className={`${styles.main} ${inter.className}`}>
        <h1>Portal para analisar o Broker em tempo real <br />("speedometro")</h1>
        <div className={`${styles.div} ${inter.className}`}>
          {result}
        </div>
      </main>
    </>
  )
}
