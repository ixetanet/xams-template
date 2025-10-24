import { useAuth } from "@ixeta/headless-auth-react";
import React from "react";

const ProtectedPage = () => {
  const auth = useAuth();

  if (!auth.isReady) {
    return <div>Loading...</div>;
  }

  if (!auth.isLoggedIn) {
    return <div>You are not logged in</div>;
  }

  return <div>Protected Page</div>;
};

export default ProtectedPage;
