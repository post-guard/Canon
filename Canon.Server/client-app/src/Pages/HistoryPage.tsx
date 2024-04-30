import {
    Box,
    Button,
    Card,
    CardActionArea,
    Dialog, DialogActions,
    DialogContent, DialogContentText,
    DialogTitle,
    Drawer,
    Stack,
    Typography
} from "@mui/material";
import createClient from "openapi-fetch";
import * as openapi from "../openapi";
import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import {enqueueSnackbar} from "notistack";

const client = createClient<openapi.paths>();


// @ts-expect-error ...
export function HistoryPage({state, setState}) {
    const [data, setData] = useState<openapi.components["schemas"]["CompileResponse"][]>([]);
    const [deleteDialog, setDeleteDialog] = useState(false);
    const navigate = useNavigate();
    useEffect(() => {

        const getResponse = async () => {
            await client.GET("/api/Compiler", {
                params:
                    {
                        query: {
                            start: 1,
                            end: 20
                        }
                    }
            }).then((response) => {
                if (response!==undefined && response.data !== undefined) {
                    setData(response.data)
                }
            });
        }
        getResponse();

    }, [state])

    const toggleDrawerClose = () => {
        setState(false);
    }

    const deleteHistory = async () => {
        await client.DELETE("/api/Compiler")
            .then((res) => {
                    if (res.response.status === 204) {
                        enqueueSnackbar("删除缓存成功", {
                            variant: "success",
                            anchorOrigin: {vertical: 'bottom', horizontal: 'right'}
                        });
                        navigate('/');
                    } else {
                        enqueueSnackbar("删除缓存失败", {
                            variant: "error",
                            anchorOrigin: {vertical: 'bottom', horizontal: 'right'}
                        });
                    }
                }
            );
    }

    const onDeleteDialogClose = () => {
        setDeleteDialog(false);
    };


    return <>
        <Drawer
            anchor={"right"}
            open={state}
            onClose={toggleDrawerClose}
        >

            <Stack spacing={2}
                   sx={{height: "100%", overflowY: "scroll", padding: "5%"}}
                   direction="column">
                {
                    data.map((item, index) => {
                            return <Card key={index}
                                         sx={{width: "100%", height: "auto",overflow: "visible"}}>
                                <CardActionArea onClick={() => {
                                    navigate(`/${item.id}`)
                                }}>
                                    <Box sx={{padding: "5%"}}>
                                        <Typography variant="h5" gutterBottom>
                                            {item.compileTime}
                                        </Typography>
                                        {item.id}
                                    </Box>
                                </CardActionArea>
                            </Card>
                        }
                    )
                }
            </Stack>
            <Button sx={{width: "20rem"}}
                    onClick={() => setDeleteDialog(true)}
                    size={"large"}>
                清除缓存
            </Button>
            <Dialog
                open={deleteDialog}
                onClose={onDeleteDialogClose}
                aria-labelledby="alert-dialog-title"
                aria-describedby="alert-dialog-description"
            >
                <DialogTitle id="alert-dialog-title">
                    {"是否清除历史记录?"}
                </DialogTitle>
                <DialogContent>
                    <DialogContentText id="alert-dialog-description">
                        编译历史记录将会被不可恢复地删除，请谨慎操作!
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={onDeleteDialogClose} autoFocus>不清除</Button>
                    <Button onClick={() => {
                        deleteHistory().then(
                            () => {
                                onDeleteDialogClose();
                                toggleDrawerClose()
                            }
                        )
                    }}>
                        清除
                    </Button>
                </DialogActions>
            </Dialog>
        </Drawer>
    </>
}
