import { withAuth } from "next-auth/middleware";

// TODO: This is a hack to work around an issue where the middleware auth redirect was triggering the nextauth login screen instead of directly showing the auth0 login screen.
// Remove this pages setting once https://github.com/nextauthjs/next-auth/discussions/4511 is fixed.
export default withAuth({
  pages: {
    signIn: "/auth/signin",
  },
});

export const config = { matcher: ["/(.+)"] };
