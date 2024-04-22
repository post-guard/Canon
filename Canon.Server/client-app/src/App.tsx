import { RouterProvider, createBrowserRouter} from 'react-router-dom'
import {Index} from "./Pages/Index";
import 'react-photo-view/dist/react-photo-view.css';
import {loader} from "./Pages/Loader.tsx";


const routers = createBrowserRouter([
    {
        path: "/",
        element: <Index/>,
    },
    {
        path: "/:compileId",
        element: <Index/>,
        loader : loader
    },
])

export function App() {
    return <>
       <RouterProvider router={routers} />
    </>
}
