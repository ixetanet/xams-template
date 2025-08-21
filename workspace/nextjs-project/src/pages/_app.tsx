import "@/styles/globals.css";
import type { AppProps } from "next/app";
import "@mantine/core/styles.css";
import "@mantine/dates/styles.css";
import "@mantine/notifications/styles.css";
import { Notifications } from "@mantine/notifications";
import {
  createTheme,
  MantineColorsTuple,
  MantineProvider,
} from "@mantine/core";
import {
  AppContextProvider,
  AuthContextProvider as XamsAuthContextProvider,
  getQueryParam,
} from "@ixeta/xams";
import "@ixeta/xams/styles.css";
import "@ixeta/xams/global.css";
import { useRouter } from "next/router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "@ixeta/headless-auth-react";
import { initializeApp } from "firebase/app";
import { getFirebaseConfig } from "@/FirebaseConfig";
import { getAuth, sendEmailVerification } from "firebase/auth";
import { FirebaseAuthConfig } from "@ixeta/headless-auth-react-firebase";

export const fireBaseApp = initializeApp(
  getFirebaseConfig(process.env.NEXT_PUBLIC_ENVIRONMENT)
);
export const fireBaseAuth = getAuth(fireBaseApp);

const queryClient = new QueryClient();

const brandcolors: MantineColorsTuple = [
  "#e3fcff",
  "#d5f2f6",
  "#b0e3e9",
  "#87d2dc",
  "#66c5d1",
  "#50bcca",
  "#41b8c7",
  "#2ea2b0",
  "#1c909e",
  "#007d8b",
];

const theme = createTheme({
  colors: {
    brandcolors,
  },
  primaryColor: "brandcolors",
  black: "#374151",
});

const firebaseAuthConfig = new FirebaseAuthConfig(fireBaseAuth);

export default function App({ Component, pageProps }: AppProps) {
  const router = useRouter();
  const userId = getQueryParam("userid", router.asPath);

  firebaseAuthConfig.setOptions({
    // What's displayed in TOTP app
    totpAppName: "ProductName",
    onSignUpSuccess: async (authConfig) => {
      if (fireBaseAuth.currentUser) {
        await sendEmailVerification(fireBaseAuth.currentUser);
      }
    },
    onSignInSuccess: async () => {
      // router.push("/app");
    },
    onSignOutSuccess: async () => {
      // router.push("/");
    },
  });

  return (
    <QueryClientProvider client={queryClient}>
      <MantineProvider theme={theme}>
        <AuthProvider authConfig={firebaseAuthConfig}>
          <XamsAuthContextProvider
            apiUrl={process.env.NEXT_PUBLIC_API as string}
            headers={{
              UserId: userId as string,
            }}
            // withCredentials={true}
            onUnauthorized={() => {
              firebaseAuthConfig.signOut();
              if (router.isReady) {
                router.push("/");
              }
            }}
            getAccessToken={firebaseAuthConfig.getAccessToken}
          >
            <AppContextProvider>
              <Notifications />
              <Component {...pageProps} />
            </AppContextProvider>
          </XamsAuthContextProvider>
        </AuthProvider>
      </MantineProvider>
    </QueryClientProvider>
  );
}
