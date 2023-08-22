import { createSignalRContext } from "react-signalr";
import { Navbar } from "./Navbar";
import { useSession } from "next-auth/react";
import dynamic from "next/dynamic";
import { ProviderProps } from "react-signalr/lib/signalr/provider";
import { OptionalExcept } from "../lib/utils";

export const Layout = dynamic(() => Promise.resolve(LayoutNonSSR), {
  ssr: false,
});

export const { useSignalREffect, Provider } = createSignalRContext();

const signalRProps: OptionalExcept<ProviderProps, "url"> = {
  connectEnabled: false,
  url: "https://127.0.0.1:7082/hub",
};

export function LayoutNonSSR({ children }: { children: React.ReactNode }) {
  const { status, data } = useSession();

  if (status === "authenticated" && data.accessToken) {
    signalRProps.connectEnabled = true;
    signalRProps.accessTokenFactory = () => data.accessToken ?? "";
  }

  return (
    <Provider {...signalRProps}>
      <Navbar />
      <div className="lg:pl-72">
        <div className="mx-auto h-[100vh] space-y-8 px-2 pt-20 lg:p-8">
          <div className="h-full rounded-lg p-px shadow-lg shadow-black/20">
            <div className="h-full rounded-lg bg-black p-3.5 lg:p-6">{children}</div>
          </div>
        </div>
      </div>
    </Provider>
  );
}
