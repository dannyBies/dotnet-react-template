import { signIn, useSession } from "next-auth/react";
import { useEffect } from "react";
import { useRouter } from "next/router";
import { useSearchParams } from "next/navigation";

// TODO: This is a hack to work around an issue where the middleware auth redirect was triggering the nextauth login screen instead of directly showing the auth0 login screen.
// Remove this file once https://github.com/nextauthjs/next-auth/discussions/4511 is fixed.
export default function Signin() {
  const router = useRouter();
  const { status } = useSession();
  const searchParams = useSearchParams();
  const callbackUrl = searchParams.get("callbackUrl");

  useEffect(() => {
    if (status === "unauthenticated") {
      void signIn("auth0");
    } else if (status === "authenticated") {
      if (callbackUrl) {
        void router.push(callbackUrl);
      } else {
        void router.push("/");
      }
    }
  }, [callbackUrl, router, status]);

  return <div></div>;
}
