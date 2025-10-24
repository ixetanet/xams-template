import { User } from "@/api/types";
import { useAuthRequest } from "@ixeta/xams";
import { useQuery } from "@tanstack/react-query";
import { useRouter } from "next/router";
import React from "react";

const useWhoAmI = () => {
  const authRequest = useAuthRequest();
  const query = useQuery<User>({
    queryKey: ["whoami"],
    queryFn: async () => {
      const resp = await authRequest.whoAmI<User>();
      return resp.data;
    },
  });

  const Error = () => {
    return (
      <div className="w-full h-full flex justify-center items-center">
        Error loading user information
      </div>
    );
  };

  return {
    query,
    isLoading: query.isLoading,
    isError: query.isError,
    data: query.data,
    Error,
  };
};

export default useWhoAmI;
