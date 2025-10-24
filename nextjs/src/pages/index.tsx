import useWhoAmI from "@/hooks/useWhoAmI";
import { Button } from "@mantine/core";
import { useRouter } from "next/router";

export default function Home() {
  const router = useRouter();
  return (
    <div className=" bg-white">
      <Button onClick={() => router.push("/login")}>Login</Button>
    </div>
  );
}
