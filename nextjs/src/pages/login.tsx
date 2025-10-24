import React from "react";
import { useRouter } from "next/router";
import { LoginPage } from "@ixeta/xams-firebase";

const Login = () => {
  const router = useRouter();
  return (
    <LoginPage
      onLoginSuccess={() => {
        const redirectUrl = localStorage.getItem("postLoginRedirect");
        localStorage.removeItem("postLoginRedirect");
        router.push(redirectUrl || "/app/coupons");
      }}
    />
  );
};

export default Login;
