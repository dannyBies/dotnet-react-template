import { useState } from "react";
import { useSignalREffect } from "../components/Layout";

export default function Page() {
  const [messages, setMessage] = useState<string[]>([]);
  useSignalREffect(
    "NewMessage",
    (message) => {
      setMessage([...messages, message]);
    },
    [messages],
  );
  return (
    <>
      {messages.map((message) => {
        return (
          <p className="bg-white" key={message}>
            {message}
          </p>
        );
      })}
    </>
  );
}
