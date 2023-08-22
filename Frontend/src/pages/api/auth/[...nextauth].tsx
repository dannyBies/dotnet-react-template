import NextAuth from "next-auth";
import Auth0Provider from "next-auth/providers/auth0";
import { env } from "@/src/env.mjs";

export default NextAuth({
  callbacks: {
    async jwt({ token, account }) {
      if (account) {
        token["accessToken"] = account.access_token;
      }
      return token;
    },
    async session({ session, token }) {
      if (token) {
        return {
          ...session,
          accessToken: token["accessToken"],
        };
      }

      return {
        ...session,
        accessToken: null,
      };
    },
  },
  providers: [
    Auth0Provider({
      clientId: env.AUTH0_CLIENT_ID,
      clientSecret: env.AUTH0_CLIENT_SECRET,
      issuer: env.AUTH0_ISSUER_BASE_URL,
      idToken: true,
      token: {
        params: {
          audience: env.AUTH0_AUDIENCE,
        },
      },
      authorization: {
        params: {
          prompt: "login",
          audience: encodeURI(env.AUTH0_AUDIENCE),
        },
      },
    }),
  ],
});
