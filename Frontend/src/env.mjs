import { createEnv } from "@t3-oss/env-nextjs";
import { z } from "zod";

export const env = createEnv({
  server: {
    AUTH0_CLIENT_ID: z.string(),
    AUTH0_CLIENT_SECRET: z.string(),
    AUTH0_ISSUER_BASE_URL: z.string().url(),
    AUTH0_AUDIENCE: z.string(),
  },
  client: {},
  experimental__runtimeEnv: {},
});
