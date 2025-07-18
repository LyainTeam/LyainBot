![LyainBot](https://socialify.git.ci/LyainTeam/LyainBot/image?custom_language=C%23&description=1&forks=1&issues=1&language=1&logo=https%3A%2F%2Favatars.githubusercontent.com%2Fu%2F221373103&name=1&owner=1&pulls=1&stargazers=1&theme=Auto)

---
*Please notice that the avatar is not created by us. Origin: [Pixiv](https://www.pixiv.net/artworks/132481358)*


## Docker Deployment

- `docker run --rm -it -v ./config:/config ghcr.io/lyainteam/lyainbot:latest`
- Fill in AppId and AppHash in `./config/client_config.json` and save file.
- Return to terminal, press Enter. Fill in phone number and Telegram login code.
- Wait until log-in finished, then press Ctrl-c.
- `docker run -d --name=lyainbot -v ./config:/config ghcr.io/lyainteam/lyainbot:latest`


