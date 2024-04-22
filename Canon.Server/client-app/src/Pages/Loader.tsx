import {redirect} from "react-router-dom";
import createClient from "openapi-fetch";
import * as openapi from "../openapi";

export async function loader() {
    const client = createClient<openapi.paths>();
    const compileId = location.pathname.substring(1);
    console.log("hello")

    const compileInstance = await client.GET("/api/Compiler/{compileId}", {
        params:
            {
                path:
                    {
                        compileId: compileId
                    }
            }
    })

    if (compileInstance.response.status !== 200) {
        // 不存在的id
        console.log("redirect")
        return redirect("/");
    }
    return null;
}
